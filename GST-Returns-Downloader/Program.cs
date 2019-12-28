using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.ViewModels;
using Devil7.Automation.GSTR.Downloader.Views;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseDataGrid()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ObservableCollectionSink(viewModel.LogEvents)
                .CreateLogger();

            var window = new MainWindow
            {
                DataContext = viewModel
            };

            Log.Verbose("Starting Application...");

            app.Run(window);
        }
    }
}
