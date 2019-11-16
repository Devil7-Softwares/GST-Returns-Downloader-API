﻿using System;
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
            this.RefreshCaptcha = ReactiveCommand.CreateFromTask (refreshCaptcha);
        }
        #endregion

        #region Variables
        private Random Random;
        private RestClient Client;

        private string username = "";
        private string password = "";

        private Bitmap captchaImage = null;
        private string captcha = "";
        private string registeredName = "";
        private string registeredGSTIN = "";
        private bool isBusy = false;
        private string status = "";
        #endregion

        #region Properties
        public string Username { get => username; set => this.RaiseAndSetIfChanged (ref username, value); }
        public string Password { get => password; set => this.RaiseAndSetIfChanged (ref password, value); }
        public Bitmap CaptchaImage { get => captchaImage; set => this.RaiseAndSetIfChanged (ref captchaImage, value); }
        public string Captcha { get => captcha; set => this.RaiseAndSetIfChanged (ref captcha, value); }

        public string RegisteredName { get => registeredName; }
        public string RegisteredGSTIN { get => registeredGSTIN; }

        public bool IsBusy { get => isBusy; set => this.RaiseAndSetIfChanged (ref isBusy, value); }
        public string Status { get => status; set => this.RaiseAndSetIfChanged (ref status, value); }
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
                        this.RefreshCaptcha.Execute ().Subscribe ();
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

        public ReactiveCommand<Unit, Unit> RefreshCaptcha { get; }
        private Task refreshCaptcha () {
            return Task.Run (() => {
                this.IsBusy = true;
                this.Status = "Loading Captcha...";

                try {
                    UpdateURL (URLs.ServicesURL);

                    RestRequest CaptchaRequest = new RestRequest (URLs.Captcha, Method.GET);
                    CaptchaRequest.AddParameter ("rnd", Random.NextDouble ());
                    RestResponse CaptchaResponse = (RestResponse) Client.Execute (CaptchaRequest);
                    if (CaptchaResponse.IsSuccessful) {
                        this.CaptchaImage = new Bitmap (new System.IO.MemoryStream (CaptchaResponse.RawBytes));
                        Console.WriteLine ("Captcha Request Successful!");
                    } else {
                        Console.WriteLine (CaptchaResponse.ErrorMessage);
                    }
                } catch (Exception ex) {
                    Console.WriteLine ("Error on refreshing captcha: " + ex.Message);
                } finally {
                    this.IsBusy = false;
                }
            });
        }
        #endregion

        #region "MiscMethods"
        private void UpdateURL (string url) {
            if (this.Client != null && (this.Client.BaseUrl == null || this.Client.BaseUrl.ToString () != url)) {
                this.Client.BaseUrl = new Uri (url);
            }
        }
        #endregion
    }
}