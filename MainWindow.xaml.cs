using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;

namespace HemanWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void ThreadNativeButtonClick(object sender, RoutedEventArgs e)
        {
            HideStoryboard.Begin();
            await Task.Delay(1);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var fileName = HemanApi.RenderNativeExample();
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            TextOutput.Text = "RunTime " + elapsedTime;
            await LoadImage(fileName);

            ShowStoryboard.Begin();
        }
        private async void ThreadButtonClick(object sender, RoutedEventArgs e)
        {
            HideStoryboard.Begin();
            await Task.Delay(1);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileName = HemanApi.RenderExample();

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            TextOutput.Text = "RunTime " + elapsedTime;
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
            OutputImage.Visibility = Visibility.Visible;
        }
    }
}
