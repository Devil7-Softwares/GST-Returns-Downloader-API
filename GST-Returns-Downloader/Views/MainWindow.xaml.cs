using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Devil7.Automation.GSTR.Downloader.Controls;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.Models;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader.Views
{
    public class MainWindow : Window
    {
        DataGrid dataGrid;
        TextBox txtUsername;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.downloadManager = this.FindControl<DownloadManager>("DownloadManager");
            this.dataGrid = this.FindControl<DataGrid>("dg_Logs");
            this.txtUsername = this.FindControl<TextBox>("txt_Username");
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

            if (dataGrid.Items is ObservableCollection<LogEvent>)
            {
                (dataGrid.Items as ObservableCollection<LogEvent>).CollectionChanged += (object collection, System.Collections.Specialized.NotifyCollectionChangedEventArgs ce) => { 
                    if (ce.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (Dispatcher.UIThread.CheckAccess())
                            ScrollToBottom();
                        else
                            Dispatcher.UIThread.InvokeAsync(() => ScrollToBottom()); 
                    }
                };
            }

            if (txtUsername != null)
            {
                txtUsername.KeyUp += TxtUsername_KeyUp;
            }
        }

        private void TxtUsername_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            string clipBoardText = Application.Current.Clipboard.GetTextAsync().Result;
            if (e.Key == Avalonia.Input.Key.V && e.Modifiers == Avalonia.Input.InputModifiers.Control && clipBoardText.Contains("\t"))
            {
                e.Handled = true;
                this.ViewModel.Username = clipBoardText.Split("\t")[0].Trim();
                this.ViewModel.Password = clipBoardText.Split("\t")[1].Trim();
            }
        }
        #endregion

        #region PrivateMethods
        private void ScrollToBottom()
        {
            if (dataGrid != null && !dataGrid.IsFocused)
            {
                if (dataGrid.Items is ObservableCollection<LogEvent> items && items.Count > 1 && dataGrid.Columns.Count > 0)
                    dataGrid.ScrollIntoView(items[items.Count - 1], dataGrid.Columns[0]);
            }
        }
        #endregion
    }
}