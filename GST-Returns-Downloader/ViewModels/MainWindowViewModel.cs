using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Devil7.Automation.GSTR.Downloader.Misc;
using Newtonsoft.Json;
using ReactiveUI;
using RestSharp;

namespace Devil7.Automation.GSTR.Downloader.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        #region Consturctor
        public MainWindowViewModel () {
            this.Random = new Random ();

            this.InitializeAPI = ReactiveCommand.CreateFromTask (initializeAPI);
            this.RefreshCaptcha = ReactiveCommand.CreateFromTask (refreshCaptcha);
            this.Authendicate = ReactiveCommand.CreateFromTask (authendicate);
            this.KeepAlive = ReactiveCommand.CreateFromTask<string> (keepAlive);
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

        public ReactiveCommand<Unit, Unit> Authendicate { get; }
        private Task authendicate () {
            return Task.Run (() => {
                try {
                    this.IsBusy = true;
                    this.Status = "Logging in...";

                    UpdateURL (URLs.ServicesURL);

                    RestRequest AuthendicateRequest1 = new RestRequest (URLs.Authendicate, Method.POST);
                    AuthendicateRequest1.AddJsonBody (new Misc.AuthenticationData (this.Username, this.Password, this.Captcha));
                    RestResponse AuthendicateResponse1 = (RestResponse) Client.Execute (AuthendicateRequest1);
                    if (AuthendicateResponse1.IsSuccessful) {
                        Models.AuthResponse authResponse = JsonConvert.DeserializeObject<Models.AuthResponse> (AuthendicateResponse1.Content);
                        if (authResponse.message != "auth") {
                            Console.Write ("Authendication Failed! ");

                            switch (authResponse.errorCode) {
                                case "SWEB_9006":
                                    Console.WriteLine ("Server Busy!");
                                    break;
                                case "AUTH_9033":
                                    Console.WriteLine ("Password has expired!");
                                    break;
                                case "AUTH_9033_MIG":
                                    Console.WriteLine ("Password has expired (Mirgration)!");
                                    break;
                                case "SWEB_9000":
                                case "SWEB_9034":
                                    Console.WriteLine ("Invalid Captcha!");
                                    RefreshCaptcha.Execute ().Subscribe ();
                                    break;
                                case "SWEB_9036":
                                    Console.WriteLine ("Invalid R0 user!");
                                    break;
                                case "AUTH_9002":
                                    Console.WriteLine ("Invalid Username or Password!");
                                    RefreshCaptcha.Execute ().Subscribe ();
                                    break;
                                case "SWEB_9014":
                                    Console.WriteLine ("System Error!");
                                    break;
                                case "SWEB_8000":
                                    Console.WriteLine ("Too Many (3) Wrong Attempts! Account Locked!");
                                    break;
                            }
                            return;
                        } else {
                            Console.WriteLine ("Authendication Phase 1 Failed!");
                        }
                    } else {
                        Console.WriteLine ("Authendication Phase 1 Request Unsuccessful!");
                        return;
                    }

                    RestRequest AuthendicateRequest2 = new RestRequest (URLs.Auth, Method.GET);
                    AuthendicateRequest2.AddCookie ("Lang", "en");
                    AuthendicateRequest2.AddHeader ("Referer", URLs.LoginURL);
                    AuthendicateRequest2.AddHeader ("Upgrade-Insecure-Requests", "1");
                    RestResponse AuthendicateResponse2 = (RestResponse) Client.Execute (AuthendicateRequest2);
                    if (!AuthendicateResponse2.IsSuccessful) {
                        Console.WriteLine ("Authendication Phase 2 Request Unsucessful!");
                        return;
                    }

                    KeepAlive.Execute (URLs.WelcomeURL).Subscribe ();

                    Console.WriteLine("Authendication Success!");
                } catch (Exception ex) {
                    Console.WriteLine ("Error on authendicating: " + ex.Message);
                } finally {
                    this.IsBusy = false;
                }
            });
        }

        public ReactiveCommand<string, Unit> KeepAlive { get; }
        private Task keepAlive (string referer) {
            return Task.Run (() => {
                try {
                    string urlBase = "";
                    if (referer.StartsWith (URLs.ServicesURL)) {
                        urlBase = "services";
                        UpdateURL (URLs.ServicesURL);
                    } else if (referer.StartsWith (URLs.ReturnsURL)) {
                        urlBase = "returns";
                        UpdateURL (URLs.ReturnsURL);
                    }
                    RestRequest KeepAliveRequest = new RestRequest (string.Format (URLs.KeepAlive, urlBase), Method.GET);
                    KeepAliveRequest.AddCookie ("Lang", "en");
                    KeepAliveRequest.AddHeader ("Referer", referer);
                    RestResponse KeepAliveResponse = (RestResponse) this.Client.Execute (KeepAliveRequest);
                    if (KeepAliveResponse.IsSuccessful) {
                        Models.AuthResponse authResponse = JsonConvert.DeserializeObject<Models.AuthResponse> (KeepAliveResponse.Content);

                        if (authResponse.successCode != "true")
                            Console.WriteLine ("Keep alive request failed!");
                        else
                            Console.WriteLine ("Keep alive request success!");
                    } else {
                        Console.WriteLine ("Keep alive request failed!");
                    }
                } catch (Exception ex) {
                    Console.WriteLine ("Error on keep alive request: " + ex.Message);
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