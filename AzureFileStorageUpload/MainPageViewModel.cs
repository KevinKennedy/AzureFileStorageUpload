using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace AzureFileStorageUpload
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private const string connectionStringKey = "#connectionString";
        private const string remoteShareNameKey = "#remoteShareName";

        private StorageFile fileToUpload = null;

        private string connectionString = null;
        public string ConnectionString
        {
            get { return this.connectionString; }
            set
            {
                if(this.PropertyChangedHelper(value, ref this.connectionString))
                {
                    ApplicationData.Current.LocalSettings.Values[connectionStringKey] = value;
                }
            }
        }

        private string remoteShareName = null;
        public string RemoteShareName
        {
            get { return this.remoteShareName; }
            set
            {
                if(this.PropertyChangedHelper(value, ref this.remoteShareName))
                {
                    ApplicationData.Current.LocalSettings.Values[remoteShareNameKey] = value;
                }

            }
        }

        public string FileToUploadName
        {
            get
            {
                if(this.fileToUpload != null)
                {
                    return this.fileToUpload.Name;
                }
                return string.Empty;
            }
        }

        public ICommand SelectFileToUpload { get; private set; }

        public ICommand UploadFile { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel()
        {
            this.SetupCommands();
            this.LoadSettings();
        }

        private void SetupCommands()
        {
            this.SelectFileToUpload = new Command(async (parameter) =>
            {
                await this.ShowFilePickerAsync();
            });

            this.UploadFile = new Command(async (parameter) =>
            {
                await this.UploadFileAsync();
            });
        }

        private void LoadSettings()
        {
            object settingObject;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(connectionStringKey, out settingObject))
            {
                if (settingObject is string)
                {
                    this.ConnectionString = (string)settingObject;
                }
            }

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(remoteShareNameKey, out settingObject))
            {
                if (settingObject is string)
                {
                    this.RemoteShareName = (string)settingObject;
                }
            }
        }

        private async Task ShowFilePickerAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                this.fileToUpload = file;
                SendPropertyChanged("FileToUploadName");
            }
        }

        private async Task UploadFileAsync()
        {
            if (string.IsNullOrEmpty(this.FileToUploadName))
            {
                return;
            }
            var remoteFileName = Path.GetFileName(this.FileToUploadName);

            //
            // This is the interesting code for the sample
            //

            string storageConnectionString = this.ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
            CloudFileDirectory cfDir = fileClient.GetShareReference(this.RemoteShareName).GetRootDirectoryReference();
            CloudFile cloudFile = cfDir.GetFileReference(remoteFileName);

            try
            {
                var fileStream = await this.fileToUpload.OpenStreamForReadAsync();
                await cloudFile.UploadFromStreamAsync(fileStream);
            }
            catch (StorageException)
            {
                // TODO - user actionable error - could be auth, missing file, server down, etc.
            }

        }

        private bool PropertyChangedHelper<T>(T newValue, ref T storage, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (IEquatable<T>.Equals(newValue, storage))
            {
                return false;
            }

            storage = newValue;
            this.SendPropertyChanged(propertyName);
            return true;
        }

        private void SendPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
