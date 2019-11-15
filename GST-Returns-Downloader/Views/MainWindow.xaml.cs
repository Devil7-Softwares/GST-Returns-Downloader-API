using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Devil7.Automation.GSTR.Downloader.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}