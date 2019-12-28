using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Devil7.Automation.GSTR.Downloader.Controls;
using Devil7.Automation.GSTR.Downloader.Misc;
using Devil7.Automation.GSTR.Downloader.Models;
using Newtonsoft.Json;
using ReactiveUI;
using RestSharp;

namespace Devil7.Automation.GSTR.Downloader.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Consturctor
        public MainWindowViewModel()
        {
            this.Random = new Random();
            this.LogEvents = new ObservableCollection<LogEvent>();

            this.InitializeAPI = ReactiveCommand.CreateFromTask<CommandResult>(initializeAPI);
            this.RefreshCaptcha = ReactiveCommand.CreateFromTask<CommandResult>(refreshCaptcha);
            this.Authendicate = ReactiveCommand.CreateFromTask<CommandResult>(authendicate);
            this.KeepAlive = ReactiveCommand.CreateFromTask<string>(keepAlive);
            this.GetMonths = ReactiveCommand.CreateFromTask<CommandResult>(getMonths);
            this.GetUserStatus = ReactiveCommand.CreateFromTask<CommandResult>(getUserStatus);
            this.StartProcess = ReactiveCommand.CreateFromTask(startProcess);

            this.LoadReturnsDatas();
        }
        #endregion

        #region Variables
        private Random Random;
        private RestClient Client;
        private DownloadManager downloadManager;

        private string username = "";
        private string password = "";

        private Bitmap captchaImage = null;
        private string captcha = "";
        private string registeredName = "";
        private string registeredGSTIN = "";
        private bool isBusy = false;
        private string status = "";
        private ObservableCollection<YearData> returnPeriods;
        private bool cancelable = false;
        private ObservableCollection<ReturnsData> returnsDatas;
        #endregion

        #region Properties
        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }
        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }
        public Bitmap CaptchaImage
        {
            get => captchaImage;
            set => this.RaiseAndSetIfChanged(ref captchaImage, value);
        }
        public string Captcha
        {
            get => captcha;
            set => this.RaiseAndSetIfChanged(ref captcha, value);
        }

        public string RegisteredName
        {
            get => registeredName;
            set => this.RaiseAndSetIfChanged(ref registeredName, value);
        }
        public string RegisteredGSTIN
        {
            get => registeredGSTIN;
            set => this.RaiseAndSetIfChanged(ref registeredGSTIN, value);
        }

        public bool IsBusy
        {
            get => isBusy;
            set => this.RaiseAndSetIfChanged(ref isBusy, value);
        }
        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        public ObservableCollection<YearData> ReturnPeriods
        {
            get => returnPeriods;
            set => this.RaiseAndSetIfChanged(ref returnPeriods, value);
        }
        public bool Cancelable
        {
            get => cancelable;
            set => this.RaiseAndSetIfChanged(ref cancelable, value);
        }
        public ObservableCollection<ReturnsData> ReturnsDatas
        {
            get => returnsDatas;
            set => this.RaiseAndSetIfChanged(ref returnsDatas, value);
        }
        public ObservableCollection<LogEvent> LogEvents { get; set; }
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> Cancel;
        public ReactiveCommand<Unit, CommandResult> InitializeAPI
        {
            get;
        }
        private Task<CommandResult> initializeAPI()
        {
            return Task.Run<CommandResult>(() =>
            {
                Models.CommandResult result = new CommandResult(CommandResult.Results.Success, "Successfully initialized API.");

                this.IsBusy = true;
                this.Status = "Initializing API...";
                this.Cancelable = false;

                try
                {
                    this.Random = new Random();
                    this.Client = new RestClient(URLs.ServicesURL);
                    this.Client.CookieContainer = new System.Net.CookieContainer();
                    this.Client.UserAgent = DownloadManager.UserAgent;

                    RestRequest InitialRequest = new RestRequest(URLs.Login, Method.GET);
                    RestResponse InitialResponse = (RestResponse)Client.Execute(InitialRequest);
                    if (InitialResponse.IsSuccessful)
                    {
                        Console.WriteLine("Initialization successful!");
                        this.RefreshCaptcha.Execute().Subscribe();
                    }
                    else
                    {
                        Console.WriteLine(InitialResponse.ErrorMessage);
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Unable to initialize API. " + InitialResponse.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on initializing API: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Unable to initialize API. " + ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<Unit, CommandResult> RefreshCaptcha
        {
            get;
        }
        private Task<CommandResult> refreshCaptcha()
        {
            return Task.Run<CommandResult>(() =>
            {
                Models.CommandResult result = new CommandResult(CommandResult.Results.Success, "Successfully loaded/refreshed captcha.");

                this.IsBusy = true;
                this.Status = "Loading Captcha...";
                this.Cancelable = false;

                try
                {
                    UpdateURL(URLs.ServicesURL);

                    RestRequest CaptchaRequest = new RestRequest(URLs.Captcha, Method.GET);
                    CaptchaRequest.AddParameter("rnd", Random.NextDouble());
                    RestResponse CaptchaResponse = (RestResponse)Client.Execute(CaptchaRequest);
                    if (CaptchaResponse.IsSuccessful)
                    {
                        this.CaptchaImage = new Bitmap(new System.IO.MemoryStream(CaptchaResponse.RawBytes));
                        Console.WriteLine("Captcha Request Successful!");
                    }
                    else
                    {
                        Console.WriteLine(CaptchaResponse.ErrorMessage);
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Captcha failed to load! " + CaptchaResponse.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on refreshing captcha: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Captcha failed to load! " + ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<Unit, CommandResult> Authendicate
        {
            get;
        }
        private Task<CommandResult> authendicate()
        {
            return Task.Run<CommandResult>(() =>
            {
                Models.CommandResult result = new CommandResult(CommandResult.Results.Success, "Successfully logged in.");
                try
                {
                    this.IsBusy = true;
                    this.Status = "Logging in...";
                    this.Cancelable = false;

                    UpdateURL(URLs.ServicesURL);

                    RestRequest AuthendicateRequest1 = new RestRequest(URLs.Authendicate, Method.POST);
                    AuthendicateRequest1.AddJsonBody(new Misc.AuthenticationData(this.Username, this.Password, this.Captcha));
                    RestResponse AuthendicateResponse1 = (RestResponse)Client.Execute(AuthendicateRequest1);
                    if (AuthendicateResponse1.IsSuccessful)
                    {
                        Models.AuthResponse authResponse = JsonConvert.DeserializeObject<Models.AuthResponse>(AuthendicateResponse1.Content);
                        if (authResponse.message != "auth")
                        {
                            Console.Write("Authendication Phase 1 Failed! ");
                            result.Result = CommandResult.Results.Failed;
                            result.Message = "Login failed. ";

                            switch (authResponse.errorCode)
                            {
                                case "SWEB_9006":
                                    Console.WriteLine("Server Busy!");
                                    result.Message += "Server busy!";
                                    break;
                                case "AUTH_9033":
                                    Console.WriteLine("Password has expired!");
                                    result.Message += "Password has expired!";
                                    break;
                                case "AUTH_9033_MIG":
                                    Console.WriteLine("Password has expired (Mirgration)!");
                                    result.Message += "Password has expired (Migration)!";
                                    break;
                                case "SWEB_9000":
                                case "SWEB_9034":
                                    Console.WriteLine("Invalid Captcha!");
                                    result.Message += "Invalid captcha!";
                                    RefreshCaptcha.Execute().Subscribe();
                                    break;
                                case "SWEB_9036":
                                    Console.WriteLine("Invalid R0 user!");
                                    result.Message += "Invalid user!";
                                    break;
                                case "AUTH_9002":
                                    Console.WriteLine("Invalid Username or Password!");
                                    result.Message += "Invalid username or password!";
                                    RefreshCaptcha.Execute().Subscribe();
                                    break;
                                case "SWEB_9014":
                                    Console.WriteLine("System Error!");
                                    result.Message += "System error!";
                                    break;
                                case "SWEB_8000":
                                    Console.WriteLine("Too Many (3) Wrong Attempts! Account Locked!");
                                    result.Message += "Too many (3) wrong attempts! Account locked!";
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Authendication Phase 1 Successful!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Authendication Phase 1 Request Unsuccessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                        return result;



                    RestRequest AuthendicateRequest2 = new RestRequest(URLs.Auth, Method.GET);
                    AuthendicateRequest2.AddCookie("Lang", "en");
                    AuthendicateRequest2.AddHeader("Referer", URLs.LoginURL);
                    AuthendicateRequest2.AddHeader("Upgrade-Insecure-Requests", "1");
                    RestResponse AuthendicateResponse2 = (RestResponse)Client.Execute(AuthendicateRequest2);
                    if (!AuthendicateResponse2.IsSuccessful)
                    {
                        Console.WriteLine("Authendication Phase 2 Request Unsucessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                        return result;



                    KeepAlive.Execute(URLs.WelcomeURL).Subscribe();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on authendicating: " + ex.Message);
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Login failed! " + ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<string, Unit> KeepAlive
        {
            get;
        }
        private Task keepAlive(string referer)
        {
            return Task.Run(() =>
            {
                try
                {
                    string urlBase = "";
                    if (referer.StartsWith(URLs.ServicesURL))
                    {
                        urlBase = "services";
                        UpdateURL(URLs.ServicesURL);
                    }
                    else if (referer.StartsWith(URLs.ReturnsURL))
                    {
                        urlBase = "returns";
                        UpdateURL(URLs.ReturnsURL);
                    }
                    RestRequest KeepAliveRequest = new RestRequest(string.Format(URLs.KeepAlive, urlBase), Method.GET);
                    KeepAliveRequest.AddCookie("Lang", "en");
                    KeepAliveRequest.AddHeader("Referer", referer);
                    RestResponse KeepAliveResponse = (RestResponse)this.Client.Execute(KeepAliveRequest);
                    if (KeepAliveResponse.IsSuccessful)
                    {
                        Models.AuthResponse authResponse = JsonConvert.DeserializeObject<Models.AuthResponse>(KeepAliveResponse.Content);

                        if (authResponse.successCode != "true")
                            Console.WriteLine("Keep alive request failed!");
                        else
                            Console.WriteLine("Keep alive request success!");



                    }
                    else
                    {
                        Console.WriteLine("Keep alive request failed!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on keep alive request: " + ex.Message);
                }
            });
        }

        public ReactiveCommand<Unit, CommandResult> GetMonths
        {
            get;
        }
        private Task<CommandResult> getMonths()
        {
            return Task.Run<CommandResult>(() =>
            {
                CommandResult result = new CommandResult(CommandResult.Results.Success, "Months Fetched Successfully!");
                try
                {
                    this.IsBusy = true;
                    this.Status = "Fetching Available Months...";
                    this.Cancelable = false;

                    UpdateURL(URLs.ReturnsURL);

                    RestRequest GetMonthsRequest = new RestRequest(URLs.Months, Method.GET);
                    GetMonthsRequest.AddCookie("Lang", "en");
                    GetMonthsRequest.AddHeader("Referer", URLs.DashboardURL);
                    RestResponse GetMonthsResponse = (RestResponse)this.Client.Execute(GetMonthsRequest);
                    if (GetMonthsResponse.IsSuccessful)
                    {
                        MonthsResponseData monthsData = JsonConvert.DeserializeObject<MonthsResponseData>(GetMonthsResponse.Content);
                        if (monthsData.status == 1)
                        {
                            ObservableCollection<YearData> ReturnPeriods = new ObservableCollection<YearData>();
                            foreach (Year year in monthsData.data.Years)
                            {
                                ReturnPeriods.Add(new YearData(year));
                            }
                            this.ReturnPeriods = ReturnPeriods;
                            Console.WriteLine("Get Months Successful.");
                        }
                        else
                        {
                            result.Result = CommandResult.Results.Failed;
                            result.Message = "Get Months Request Status Failed!";
                        }
                    }
                    else
                    {
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Get Months Request Failed!";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Get Months Failed. " + ex.Message);
                    result.Message = "Get Months Failed." + ex.Message;
                    result.Result = CommandResult.Results.Failed;
                }
                finally
                {
                    this.IsBusy = false;
                }
                return result;
            });
        }

        public ReactiveCommand<Unit, CommandResult> GetUserStatus
        {
            get;
        }
        private Task<CommandResult> getUserStatus()
        {
            return Task.Run<CommandResult>(() =>
            {
                CommandResult result = new CommandResult(CommandResult.Results.Success, "Get UserStatus Successful.");

                try
                {
                    this.isBusy = true;
                    this.Status = "Fetching User Status...";
                    this.Cancelable = false;

                    UpdateURL(URLs.ReturnsURL);

                    RestRequest request = new RestRequest(URLs.UserStatus, Method.GET);
                    request.AddHeader("Referer", URLs.DashboardURL);
                    RestResponse response = (RestResponse)Client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        UserStatus userStatus = JsonConvert.DeserializeObject<UserStatus>(response.Content);
                        this.RegisteredGSTIN = userStatus.gstin;
                        this.RegisteredName = userStatus.bname;
                    }
                    else
                    {
                        string errorMessage = "Get UserStatus Request Failed!";
                        Console.WriteLine(errorMessage);
                        result.Message = errorMessage;
                        result.Result = CommandResult.Results.Failed;
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = "Get UserStatus Failed!" + ex.Message;
                    Console.WriteLine(errorMessage);
                    result.Message = errorMessage;
                    result.Result = CommandResult.Results.Failed;
                }
                finally
                {
                    this.isBusy = false;
                }

                return result;
            });
        }

        public ReactiveCommand<Unit, Unit> StartProcess
        {
            get;
        }
        private Task startProcess()
        {
            return Task.Run(() =>
            {
                foreach (YearData year in this.ReturnPeriods)
                {
                    foreach (MonthData month in year.Months)
                    {
                        if (month.IsChecked)
                        {
                            keepAlive(URLs.DashboardURL);
                            RoleStatus roleStatus = getRoleStatus(month.Value).Result;
                            if (roleStatus != null && roleStatus.status == 1 && roleStatus.data != null && roleStatus.data.user != null && roleStatus.data.user.Count > 0)
                            {
                                foreach (User user in roleStatus.data.user)
                                {
                                    if (user.returns != null && user.returns.Count > 0)
                                    {
                                        foreach (ReturnsData returns in this.ReturnsDatas)
                                        {
                                            Return returnStatus = user.returns.Find(item => item.return_ty == returns.ReturnName.Replace(" ", ""));
                                            if (returnStatus != null && returnStatus.status == "FIL" && returnStatus.tileDisable == false)
                                            {
                                                foreach (FileType fileType in returns.FileTypes)
                                                {
                                                    foreach (ReturnOperation operation in fileType.Operations)
                                                    {
                                                        if (operation.Value)
                                                        {
                                                            if (operation.Action != null)
                                                            {
                                                                CommandResult result = operation.Action(Client, month.Value);
                                                                if (result.Result == CommandResult.Results.Success)
                                                                {
                                                                    if (result.Data is List<string>)
                                                                    {
                                                                        foreach (string url in ((List<string>)result.Data))
                                                                        {
                                                                            DownloadManager.DownloadItem downloadItem = new DownloadManager.DownloadItem(url, @"D:\");
                                                                            downloadItem.CustomCookies = Client.CookieContainer.GetCookies(Client.BaseUrl);
                                                                            downloadManager.Downloads.Add(downloadItem);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private Task<RoleStatus> getRoleStatus(string monthValue)
        {
            return Task.Run<RoleStatus>(() =>
            {
                RoleStatus value = null;

                try
                {
                    this.isBusy = true;
                    this.Status = "Fetching Returns Status Details...";

                    UpdateURL(URLs.ReturnsURL);

                    RestRequest request = new RestRequest(string.Format(URLs.RoleStatus, monthValue), Method.GET);
                    request.AddCookie("Lang", "en");
                    request.AddHeader("Referer", URLs.DashboardURL);
                    RestResponse response = (RestResponse)Client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        value = JsonConvert.DeserializeObject<RoleStatus>(response.Content);
                    }
                    else
                    {
                        Console.WriteLine("Error on RoleStatus Request for " + monthValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fetch RoleStatus Failed!. " + ex.Message);
                }
                finally
                {
                    this.isBusy = false;
                }

                return value;
            });
        }
        #endregion

        #region Public Methods
        public void SetDownloadManager(DownloadManager downloadManager)
        {
            this.downloadManager = downloadManager;
        }
        #endregion

        #region Private Methods
        private void UpdateURL(string url)
        {
            if (this.Client != null && (this.Client.BaseUrl == null || this.Client.BaseUrl.ToString() != url))
            {
                this.Client.BaseUrl = new Uri(url);
            }
        }

        private void LoadReturnsDatas()
        {
            ObservableCollection<ReturnsData> returnsDatas = new ObservableCollection<ReturnsData>();

            ReturnsData GSTR1 = new ReturnsData()
            {
                ReturnName = "GSTR 1",
                FileTypes = new ObservableCollection<FileType>() {
                    new FileType() {
                        FileTypeName = "PDF",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Download",
                            }
                        }
                    },
                    new FileType() {
                        FileTypeName = "JSON",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Generate",
                                Action = DownloadMethods.GSTR1_JSON_GENERATE
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                                Action = DownloadMethods.GSTR1_JSON_DOWNLOAD
                            }
                        }
                    }
                }
            };

            ReturnsData GSTR2A = new ReturnsData()
            {
                ReturnName = "GSTR 2A",
                FileTypes = new ObservableCollection<FileType>() {
                    new FileType() {
                        FileTypeName = "Excel",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Generate",
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                            }
                        }
                    },
                    new FileType() {
                        FileTypeName = "JSON",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Generate",
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                            }
                        }
                    }
                }
            };

            ReturnsData GSTR3B = new ReturnsData()
            {
                ReturnName = "GSTR 3B",
                FileTypes = new ObservableCollection<FileType>() {
                    new FileType() {
                        FileTypeName = "PDF",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Download",
                            }
                        }
                    }
                }
            };

            ReturnsData GSTR4 = new ReturnsData()
            {
                ReturnName = "GSTR 4",
                FileTypes = new ObservableCollection<FileType>() {
                    new FileType() {
                        FileTypeName = "PDF",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Download"
                            }
                        }
                    }
                }
            };

            ReturnsData GSTR4A = new ReturnsData()
            {
                ReturnName = "GSTR 4A",
                FileTypes = new ObservableCollection<FileType>() {
                    new FileType() {
                        FileTypeName = "JSON",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Generate",
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                            }
                        }
                    }
                }
            };

            returnsDatas.Add(GSTR1);
            returnsDatas.Add(GSTR2A);
            returnsDatas.Add(GSTR3B);
            returnsDatas.Add(GSTR4);
            returnsDatas.Add(GSTR4A);

            this.ReturnsDatas = returnsDatas;
        }
        #endregion
    }
}
