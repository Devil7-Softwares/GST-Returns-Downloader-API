using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Devil7.Automation.GSTR.Downloader.Misc;
using ReactiveUI;
using RestSharp;

namespace Devil7.Automation.GSTR.Downloader.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        #region Consturctor
        public MainWindowViewModel () {
            this.Random = new Random ();

            this.InitializeAPI = ReactiveCommand.CreateFromTask (initializeAPI);
        }
        #endregion

        #region Variables
        private Random Random;
        private RestClient Client;
        #endregion

        #region Properties
        public string Username { get; set; }
        public string Password { get; set; }
        public Bitmap CaptchaImage { get; set; }
        public string Captcha { get; set; }

        public string RegisteredName { get; }
        public string RegisteredGSTIN { get; }

        public bool IsBusy { get; set; }
        public string Status { get; set; }
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> InitializeAPI { get; }
        private Task initializeAPI () {
            return Task.Run (() => {
                this.IsBusy = true;
                this.Status = "Initializing API...";

                try {
                    this.Random = new Random ();
                    this.Client = new RestClient (URLs.ServicesURL);
                    this.Client.CookieContainer = new System.Net.CookieContainer ();
                    this.Client.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36";

                    RestRequest InitialRequest = new RestRequest (URLs.Login, Method.GET);
                    RestResponse InitialResponse = (RestResponse) Client.Execute (InitialRequest);
                    if (InitialResponse.IsSuccessful) {
                        Console.WriteLine ("Initialization successful!");
                    } else {
                        Console.WriteLine (InitialResponse.ErrorMessage);
                    }
                } catch (Exception ex) {
                    Console.WriteLine ("Error on initializing API: " + ex.Message);
                } finally {
                    this.IsBusy = false;
                }
            });
        }
        #endregion
    }
}