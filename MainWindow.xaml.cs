using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;

namespace HemanWinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void ThreadButtonClick(object sender, RoutedEventArgs e)
        {
            HideStoryboard.Begin();
            await Task.Delay(1);

            var fileName = HemanApi.RenderExample();
            await LoadImage(fileName);

            ShowStoryboard.Begin();
        }

        private async Task LoadImage(string fileName)
        {
            var file = await StorageFile.GetFileFromPathAsync(fileName);
            using var fileStream = await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(file);
            BitmapImage bitmapImage = new();
            await bitmapImage.SetSourceAsync(fileStream.AsRandomAccessStream());
            OutputImage.Source = bitmapImage;
            if (OutputImage.Visibility != Visibility.Visible)
            {
                OutputImage.Visibility = Visibility.Visible;
            }
        }
    }
}
