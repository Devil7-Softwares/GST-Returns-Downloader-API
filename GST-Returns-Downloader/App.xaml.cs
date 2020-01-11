using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.ViewModels;
using Devil7.Automation.GSTR.Downloader.Views;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ObservableCollectionSink(viewModel.LogEvents)
                .CreateLogger();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };
            }

            Log.Verbose("Starting Application...");

            base.OnFrameworkInitializationCompleted();
        }
    }
}