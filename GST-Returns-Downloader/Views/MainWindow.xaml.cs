using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Devil7.Automation.GSTR.Downloader.Misc;

namespace Devil7.Automation.GSTR.Downloader.Views {
    public class MainWindow : Window {
        public MainWindow () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        #region Properties
        private ViewModels.MainWindowViewModel ViewModel {
            get {
                return ((ViewModels.MainWindowViewModel) this.DataContext);
            }
        }
        #endregion

        #region Events
        private void Window_Opened (object sender, EventArgs e) {
            this.ViewModel.InitializeAPI.Execute ().Subscribe ();

            this.ViewModel.Authendicate.Subscribe (result => { MessageBoxHelper.Show (result); });
        }
        #endregion
    }
}