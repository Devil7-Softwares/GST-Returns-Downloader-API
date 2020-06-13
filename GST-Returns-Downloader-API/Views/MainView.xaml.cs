using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Devil7.Automation.GSTR.Downloader.Controls;
using Devil7.Automation.GSTR.Downloader.Models;
using Devil7.Automation.GSTR.Downloader.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Devil7.Automation.GSTR.Downloader.Views
{
    /// <summary>
    /// Interaction logic for View1.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        readonly DispatcherTimer keepAliveTimer;
        private ObservableCollection<LogEvent> logEvents;
        private object syncLock = new object();

        public MainView()
        {
            InitializeComponent();

            this.Loaded += MainView_Loaded;
        }

        #region Properties
        private ViewModels.MainViewModel ViewModel
        {
            get
            {
                return ((ViewModels.MainViewModel)this.DataContext);
            }
        }
        #endregion

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            logEvents = new ObservableCollection<LogEvent>();
            this.ViewModel.LogEvents = CollectionViewSource.GetDefaultView(logEvents);
            BindingOperations.EnableCollectionSynchronization(logEvents, syncLock);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ObservableCollectionSink(logEvents, ref syncLock)
                .CreateLogger();

            this.ViewModel.SetDownloadManager(DownloadManager);

            this.ViewModel.InitializeAPI.Execute(null);
            
            //if (dataGrid.Items is ObservableCollection<LogEvent>)
            //{
            //    (dataGrid.Items as ObservableCollection<LogEvent>).CollectionChanged += (object collection, System.Collections.Specialized.NotifyCollectionChangedEventArgs ce) =>
            //    {
            //        if (ce.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            //        {
            //            if (Dispatcher.UIThread.CheckAccess())
            //                ScrollToBottom();
            //            else
            //                Dispatcher.UIThread.InvokeAsync(() => ScrollToBottom());
            //        }
            //    };
            //}

            if (txtUsername != null)
            {
                txtUsername.KeyUp += TxtUsername_KeyUp; ;
            }
        }

        private void TxtUsername_KeyUp(object sender, KeyEventArgs e)
        {
            string clipBoardText = Clipboard.GetText();
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && clipBoardText.Contains("\t"))
            {
                e.Handled = true;
                this.ViewModel.Username = clipBoardText.Split('\t')[0].Trim();
                this.ViewModel.Password = clipBoardText.Split('\t')[1].Trim();
            }
        }

        private void KeepAliveTimer_Tick(object sender, EventArgs e)
        {
            this.ViewModel.KeepAlive.Execute(URLs.DashboardURL);
        }
    }
}
