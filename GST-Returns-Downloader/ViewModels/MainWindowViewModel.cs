using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Devil7.Automation.GSTR.Downloader.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        public string Username { get; set; }
        public string Password { get; set; }
        public Bitmap CaptchaImage { get; set; }
        public string Captcha { get; set; }

        public string RegisteredName { get; }
        public string RegisteredGSTIN { get; }

        public bool IsBusy { get; set; }
        public string Status { get; set; }
    }
}