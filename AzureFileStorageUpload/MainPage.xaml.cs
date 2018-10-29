using Windows.UI.Xaml.Controls;

namespace AzureFileStorageUpload
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = new MainPageViewModel();
        }
    }
}
