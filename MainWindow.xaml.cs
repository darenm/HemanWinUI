using Microsoft.UI.Xaml;

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

        private void ThreadButtonClick(object sender, RoutedEventArgs e)
        {
            var hemanApi = new HemanApi();
            var threadCount = hemanApi.GetNumberOfThreads();
            ThreadInfo.Text = $"Heman API returned {threadCount} thread{(threadCount == 1 ? string.Empty: "s")}.";
        }
    }
}
