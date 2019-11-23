using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.Models;
using Newtonsoft.Json;
using ReactiveUI;
using RestSharp;

namespace Devil7.Automation.GSTR.Downloader.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        #region Consturctor
        public MainWindowViewModel () {
            this.Random = new Random ();

            this.InitializeAPI = ReactiveCommand.CreateFromTask<CommandResult> (initializeAPI);
            this.RefreshCaptcha = ReactiveCommand.CreateFromTask<CommandResult> (refreshCaptcha);
            this.Authendicate = ReactiveCommand.CreateFromTask<CommandResult> (authendicate);
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
        public ReactiveCommand<Unit, CommandResult> InitializeAPI { get; }
        private Task<CommandResult> initializeAPI () {
            return Task.Run<CommandResult> (() => {
                Models.CommandResult result = new CommandResult (CommandResult.Results.Success, "Successfully initialized API.");

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
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Unable to initialize API. " + InitialResponse.ErrorMessage;
                    }
                } catch (Exception ex) {
                    Console.WriteLine ("Error on initializing API: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Unable to initialize API. " + ex.Message;
                } finally {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<Unit, CommandResult> RefreshCaptcha { get; }
        private Task<CommandResult> refreshCaptcha () {
            return Task.Run<CommandResult> (() => {
                Models.CommandResult result = new CommandResult (CommandResult.Results.Success, "Successfully loaded/refreshed captcha.");

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
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Captcha failed to load! " + CaptchaResponse.ErrorMessage;
                    }
                } catch (Exception ex) {
                    Console.WriteLine ("Error on refreshing captcha: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Captcha failed to load! " + ex.Message;
                } finally {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<Unit, CommandResult> Authendicate { get; }
        private Task<CommandResult> authendicate () {
            return Task.Run<CommandResult> (() => {
                Models.CommandResult result = new CommandResult (CommandResult.Results.Success, "Successfully logged in.");
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
                            Console.Write ("Authendication Phase 1 Failed! ");
                            result.Result = CommandResult.Results.Failed;
                            result.Message = "Login failed. ";

                            switch (authResponse.errorCode) {
                                case "SWEB_9006":
                                    Console.WriteLine ("Server Busy!");
                                    result.Message += "Server busy!";
                                    break;
                                case "AUTH_9033":
                                    Console.WriteLine ("Password has expired!");
                                    result.Message += "Password has expired!";
                                    break;
                                case "AUTH_9033_MIG":
                                    Console.WriteLine ("Password has expired (Mirgration)!");
                                    result.Message += "Password has expired (Migration)!";
                                    break;
                                case "SWEB_9000":
                                case "SWEB_9034":
                                    Console.WriteLine ("Invalid Captcha!");
                                    result.Message += "Invalid captcha!";
                                    RefreshCaptcha.Execute ().Subscribe ();
                                    break;
                                case "SWEB_9036":
                                    Console.WriteLine ("Invalid R0 user!");
                                    result.Message += "Invalid user!";
                                    break;
                                case "AUTH_9002":
                                    Console.WriteLine ("Invalid Username or Password!");
                                    result.Message += "Invalid username or password!";
                                    RefreshCaptcha.Execute ().Subscribe ();
                                    break;
                                case "SWEB_9014":
                                    Console.WriteLine ("System Error!");
                                    result.Message += "System error!";
                                    break;
                                case "SWEB_8000":
                                    Console.WriteLine ("Too Many (3) Wrong Attempts! Account Locked!");
                                    result.Message += "Too many (3) wrong attempts! Account locked!";
                                    break;
                            }
                        } else {
                            Console.WriteLine ("Authendication Phase 1 Successful!");
                        }
                    } else {
                        Console.WriteLine ("Authendication Phase 1 Request Unsuccessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                        return result;

                    RestRequest AuthendicateRequest2 = new RestRequest (URLs.Auth, Method.GET);
                    AuthendicateRequest2.AddCookie ("Lang", "en");
                    AuthendicateRequest2.AddHeader ("Referer", URLs.LoginURL);
                    AuthendicateRequest2.AddHeader ("Upgrade-Insecure-Requests", "1");
                    RestResponse AuthendicateResponse2 = (RestResponse) Client.Execute (AuthendicateRequest2);
                    if (!AuthendicateResponse2.IsSuccessful) {
                        Console.WriteLine ("Authendication Phase 2 Request Unsucessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                        return result;

                    KeepAlive.Execute (URLs.WelcomeURL).Subscribe ();
                } catch (Exception ex) {
                    Console.WriteLine ("Error on authendicating: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Login failed! " + ex.Message;
                } finally {
                    this.IsBusy = false;
                }

                return result;
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