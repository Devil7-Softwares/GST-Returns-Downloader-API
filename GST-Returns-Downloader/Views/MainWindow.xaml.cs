using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Devil7.Automation.GSTR.Downloader.Controls;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.Models;

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

            this.downloadManager = this.FindControl<DownloadManager>("DownloadManager");
        }

        #region Properties
        private ViewModels.MainWindowViewModel ViewModel
        {
            get
            {
                return ((ViewModels.MainWindowViewModel)this.DataContext);
            }
        }

        private DownloadManager downloadManager;
        private DownloadManager DownloadManager { get => downloadManager; }
        #endregion

        #region Events
        private void Window_Opened(object sender, EventArgs e)
        {
            this.ViewModel.SetDownloadManager(downloadManager);

            this.ViewModel.InitializeAPI.Execute().Subscribe();

            this.ViewModel.InitializeAPI.Subscribe(async result =>
            {
                await MessageBoxHelper.ShowError(result, this);
                if (result.Result == CommandResult.Results.Failed) Environment.Exit(-1);
            });
            this.ViewModel.Authendicate.Subscribe(async result =>
            {
                await MessageBoxHelper.Show(result, this);
                if (result.Result == CommandResult.Results.Success)
                {
                    await this.ViewModel.GetMonths.Execute();
                }
            });
            this.ViewModel.GetMonths.Subscribe(async result =>
            {
                await MessageBoxHelper.ShowError(result, this);
                if (result.Result == CommandResult.Results.Success)
                {
                    await this.ViewModel.GetUserStatus.Execute();
                }
            });
            this.ViewModel.GetUserStatus.Subscribe(async result =>
            {
                await MessageBoxHelper.ShowError(result, this);
            });
        }
        #endregion
    }
}