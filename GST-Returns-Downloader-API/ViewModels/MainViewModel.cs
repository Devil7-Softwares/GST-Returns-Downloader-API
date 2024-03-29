﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Mvvm;
using Devil7.Automation.GSTR.Downloader.Controls;
using Devil7.Automation.GSTR.Downloader.Models;
using Devil7.Automation.GSTR.Downloader.Utils;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Consturctor
        public MainViewModel()
        {
            this.Random = new Random();
            this.downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.openFolderDialog = new FolderBrowserDialog() { SelectedPath = downloadsFolder };
            this.pdfMaker = new PDFMakeWrapper(DownloadsFolder);

            this.InitializeAPI = new AsyncCommand(InitializeAPI_Task);
            this.RefreshCaptcha = new AsyncCommand(RefreshCaptcha_Task);
            this.Authendicate = new AsyncCommand(Authendicate_Task);
            this.GetMonths = new AsyncCommand(GetMonths_Task);
            this.GetUserStatus = new AsyncCommand(GetUserStatus_Task);
            this.KeepAlive = new AsyncCommand<string>(KeepAlive_Task);
            this.StartProcess = new AsyncCommand(StartProcess_Task);
            this.SelectDownloadsFolder = new DelegateCommand(SelectDownloadsFolder_Task);

            this.LoadReturnsDatas();
        }

        private void LogEvents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine(e.NewItems.Count);
        }
        #endregion

        #region Variables
        private Random Random;
        private RestClient Client;
        private DownloadManager downloadManager;
        private readonly FolderBrowserDialog openFolderDialog;
        private readonly PDFMakeWrapper pdfMaker;

        private string username = "";
        private string password = "";

        private ImageSource captchaImage = null;
        private string captcha = "";
        private string registeredName = "";
        private string registeredGSTIN = "";
        private string tradeName = "";
        private bool isBusy = false;
        private string status = "";
        private ObservableCollection<YearData> returnPeriods;
        private bool cancelable = false;
        private ObservableCollection<ReturnsData> returnsDatas;
        private ICollectionView logEvents = null;
        private string downloadsFolder = "";
        #endregion

        #region Properties
        public string Username
        {
            get => username;
            set => this.SetProperty(ref username, value, "Username");
        }
        public string Password
        {
            get => password;
            set => this.SetProperty(ref password, value, "Password");
        }
        public ImageSource CaptchaImage
        {
            get => captchaImage;
            set => this.SetProperty(ref captchaImage, value, "CaptchaImage");
        }
        public string Captcha
        {
            get => captcha;
            set => this.SetProperty(ref captcha, value, "Captcha");
        }

        public string RegisteredName
        {
            get => registeredName;
            set => this.SetProperty(ref registeredName, value, "RegisteredName");
        }
        public string RegisteredGSTIN
        {
            get => registeredGSTIN;
            set => this.SetProperty(ref registeredGSTIN, value, "RegisteredGSTIN");
        }
        public string TradeName
        {
            get => tradeName;
            set => this.SetProperty(ref tradeName, value, "TradeName");
        }

        public bool IsBusy
        {
            get => isBusy;
            set => this.SetProperty(ref isBusy, value, "IsBusy");
        }
        public string Status
        {
            get => status;
            set => this.SetProperty(ref status, value, "Status");
        }

        public ObservableCollection<YearData> ReturnPeriods
        {
            get => returnPeriods;
            set => this.SetProperty(ref returnPeriods, value, "ReturnPeriods");
        }
        public bool Cancelable
        {
            get => cancelable;
            set => this.SetProperty(ref cancelable, value, "Cancelable");
        }
        public ObservableCollection<ReturnsData> ReturnsDatas
        {
            get => returnsDatas;
            set => this.SetProperty(ref returnsDatas, value, "ReturnsDatas");
        }

        public ICollectionView LogEvents { get => logEvents; set => SetProperty(ref logEvents, value, "LogEvents"); }

        public string DownloadsFolder { get => this.downloadsFolder; set => this.SetProperty(ref downloadsFolder, value, "DownloadsFolder"); }
        #endregion

        #region Commands
        public AsyncCommand Cancel;
        public AsyncCommand InitializeAPI
        {
            get;
        }
        private Task<CommandResult> InitializeAPI_Task()
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
                    this.Client = new RestClient(URLs.ServicesURL)
                    {
                        CookieContainer = new System.Net.CookieContainer(),
                        UserAgent = DownloadManager.UserAgent
                    };

                    RestRequest InitialRequest = new RestRequest(URLs.Login, Method.GET);
                    RestResponse InitialResponse = (RestResponse)Client.Execute(InitialRequest);
                    if (InitialResponse.IsSuccessful)
                    {
                        Log.Information("Initialization successful!");
                        App.Current.Dispatcher.Invoke(() => this.RefreshCaptcha.Execute(null));
                    }
                    else
                    {
                        string errorMsg = InitialResponse.ErrorMessage;
                        if (InitialResponse.Content != null && InitialResponse.Content.Contains("Scheduled Downtime"))
                            errorMsg = "Site under maintanence!";

                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Unable to initialize API. " + errorMsg;
                        Log.Error(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Unable to initialize API. " + ex.Message;
                    Log.Error(result.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }

                /// TODO: await MessageBoxHelper.ShowError(result, this);
                if (result.Result == CommandResult.Results.Failed) Environment.Exit(-1);

                return result;
            });
        }

        public AsyncCommand RefreshCaptcha
        {
            get;
        }
        private Task<CommandResult> RefreshCaptcha_Task()
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
                        BitmapImage captchaSource = new BitmapImage();
                        captchaSource.BeginInit();
                        captchaSource.StreamSource = new MemoryStream(CaptchaResponse.RawBytes);
                        captchaSource.EndInit();
                        captchaSource.Freeze();
                        Dispatcher.CurrentDispatcher.Invoke(() => this.CaptchaImage = captchaSource);

                        Log.Information("Captcha Request Successful!");
                    }
                    else
                    {
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Captcha failed to load! " + CaptchaResponse.ErrorMessage;
                        Log.Error(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Captcha failed to load! " + ex.Message;
                    Log.Error(ex, result.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }

                return result;
            });
        }

        public AsyncCommand Authendicate
        {
            get;
        }
        private Task<CommandResult> Authendicate_Task()
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
                    AuthendicateRequest1.AddJsonBody(new Utils.AuthenticationData(this.Username, this.Password, this.Captcha));
                    RestResponse AuthendicateResponse1 = (RestResponse)Client.Execute(AuthendicateRequest1);
                    if (AuthendicateResponse1.IsSuccessful)
                    {
                        Models.AuthResponse authResponse = JsonConvert.DeserializeObject<Models.AuthResponse>(AuthendicateResponse1.Content);
                        if (authResponse.message != "auth")
                        {
                            Log.Debug("Authendication Phase 1 Failed!");
                            result.Result = CommandResult.Results.Failed;
                            result.Message = "Login failed. ";

                            switch (authResponse.errorCode)
                            {
                                case "SWEB_9006":
                                    result.Message += "Server busy!";
                                    break;
                                case "AUTH_9033":
                                    result.Message += "Password has expired!";
                                    break;
                                case "AUTH_9033_MIG":
                                    result.Message += "Password has expired (Migration)!";
                                    break;
                                case "SWEB_9000":
                                case "SWEB_9034":
                                    result.Message += "Invalid captcha!";
                                    App.Current.Dispatcher.Invoke(() => this.RefreshCaptcha.Execute(null));
                                    break;
                                case "SWEB_9036":
                                    result.Message += "Invalid user!";
                                    break;
                                case "AUTH_9002":
                                    result.Message += "Invalid username or password!";
                                    App.Current.Dispatcher.Invoke(() => this.RefreshCaptcha.Execute(null));
                                    break;
                                case "SWEB_9014":
                                    result.Message += "System error!";
                                    break;
                                case "SWEB_8000":
                                    result.Message += "Too many (3) wrong attempts! Account locked!";
                                    break;
                            }
                        }
                        else
                        {
                            Log.Debug("Authendication Phase 1 Successful!");
                        }
                    }
                    else
                    {
                        Log.Debug("Authendication Phase 1 Request Unsuccessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                    {
                        Log.Error(result.Message);
                        return result;
                    }

                    RestRequest AuthendicateRequest2 = new RestRequest(URLs.Auth, Method.GET);
                    AuthendicateRequest2.AddCookie("Lang", "en");
                    AuthendicateRequest2.AddHeader("Referer", URLs.LoginURL);
                    AuthendicateRequest2.AddHeader("Upgrade-Insecure-Requests", "1");
                    RestResponse AuthendicateResponse2 = (RestResponse)Client.Execute(AuthendicateRequest2);
                    if (!AuthendicateResponse2.IsSuccessful)
                    {
                        Log.Debug("Authendication Phase 2 Request Unsucessful!");
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Login request failed! Unknown network error!";
                    }

                    if (result.Result == CommandResult.Results.Failed)
                        return result;

                    App.Current.Dispatcher.Invoke(() => KeepAlive.Execute(URLs.WelcomeURL));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Login failed!");
                    result.Result = CommandResult.Results.Failed;
                    result.Message = "Login failed! " + ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                if (result.Result == CommandResult.Results.Success)
                    Log.Information(result.Message);

                /// TODO: await MessageBoxHelper.Show(result, this);
                if (result.Result == CommandResult.Results.Success)
                {
                    /// TODO: keepAliveTimer.Start();
                    App.Current.Dispatcher.Invoke(() => this.GetMonths.Execute(null));
                }
                else
                {
                    /// TODO: keepAliveTimer.Stop();
                }

                return result;
            });
        }

        public AsyncCommand<string> KeepAlive { get; }
        private Task KeepAlive_Task(string referer)
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
                            Log.Warning("Keep alive request failed!");
                        else
                            Log.Information("Keep alive request success!");
                    }
                    else
                    {
                        Log.Warning("Keep alive request failed!");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Error on keep alive request: " + ex.Message);
                }
            });
        }

        public AsyncCommand GetMonths
        {
            get;
        }
        private Task<CommandResult> GetMonths_Task()
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
                            Log.Information("Get Months Successful.");
                        }
                        else
                        {
                            result.Result = CommandResult.Results.Failed;
                            result.Message = "Get Months Request Status Failed!";
                            Log.Error(result.Message);
                        }
                    }
                    else
                    {
                        result.Result = CommandResult.Results.Failed;
                        result.Message = "Get Months Request Failed!";
                        Log.Error(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.Message = "Get Months Failed." + ex.Message;
                    result.Result = CommandResult.Results.Failed;
                    Log.Error(ex, result.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }

                /// TODO: await MessageBoxHelper.ShowError(result, this);
                if (result.Result == CommandResult.Results.Success)
                {
                    App.Current.Dispatcher.Invoke(() => this.GetUserStatus.Execute(null));
                }

                return result;
            });
        }

        public AsyncCommand GetUserStatus
        {
            get;
        }
        private Task<CommandResult> GetUserStatus_Task()
        {
            return Task.Run<CommandResult>(() =>
            {
                CommandResult result = new CommandResult(CommandResult.Results.Success, "Get UserStatus Successful.");

                try
                {
                    this.IsBusy = true;
                    this.Status = "Fetching User Status...";
                    this.Cancelable = false;

                    UpdateURL(URLs.ReturnsURL);

                    RestRequest requestStatus = new RestRequest(URLs.UserStatus, Method.GET);
                    requestStatus.AddHeader("Referer", URLs.DashboardURL);
                    RestResponse responseStatus = (RestResponse)Client.Execute(requestStatus);
                    if (responseStatus.IsSuccessful)
                    {
                        UserStatus userStatus = JsonConvert.DeserializeObject<UserStatus>(responseStatus.Content);
                        this.RegisteredGSTIN = userStatus.gstin;
                        this.RegisteredName = userStatus.bname;
                        this.pdfMaker.GSTIN = userStatus.gstin;
                        this.pdfMaker.RegisteredName = userStatus.bname;

                        RestRequest requestDetails = new RestRequest(string.Format(URLs.UserRegDetails, userStatus.gstin), Method.GET);
                        requestDetails.AddHeader("Referer", URLs.Gstr1URL);
                        RestResponse responseDetails = (RestResponse)Client.Execute(requestDetails);
                        if (responseDetails.IsSuccessful)
                        {
                            UserRegDetails userRegDetails = JsonConvert.DeserializeObject<UserRegDetails>(responseDetails.Content);
                            if (userRegDetails.status == 1 && userRegDetails.data != null)
                            {
                                this.TradeName = userRegDetails.data.regName;
                                this.pdfMaker.TradeName = userRegDetails.data.regName;
                            }
                            else
                            {
                                Log.Warning("No data in UserRegDetails!");
                            }
                        }
                        else
                        {
                            result.Message = "Get UserRegDetails Request Failed!";
                            result.Result = CommandResult.Results.Failed;
                            Log.Error(result.Message);
                        }
                    }
                    else
                    {
                        result.Message = "Get UserStatus Request Failed!";
                        result.Result = CommandResult.Results.Failed;
                        Log.Error(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.Message = "Get UserStatus Failed!" + ex.Message;
                    result.Result = CommandResult.Results.Failed;
                    Log.Error(ex, result.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }

                /// TODO: await MessageBoxHelper.ShowError(result, this);

                return result;
            });
        }

        public AsyncCommand StartProcess
        {
            get;
        }
        private Task StartProcess_Task()
        {
            return Task.Run(() =>
            {
                try
                {
                    this.Status = "Starting process...";
                    this.IsBusy = true;
                    if (this.ReturnPeriods == null)
                    {
                        /// TODO: MessageBoxHelper.Show("Return periods is null. Try after successful login!", "Error");
                        return;
                    }
                    bool isReturnPeriodSelected = false;
                    foreach (YearData yearData in this.ReturnPeriods)
                    {
                        foreach (MonthData monthData in yearData.Months)
                        {
                            if (monthData.IsChecked)
                            {
                                isReturnPeriodSelected = true;
                                break;
                            }
                        }
                        if (isReturnPeriodSelected)
                            break;
                    }
                    if (!isReturnPeriodSelected)
                    {
                        /// TODO: MessageBoxHelper.Show("Please select atleast one month!", "Error");
                        return;
                    }
                    bool isReturnOperationSelected = false;
                    foreach (ReturnsData returnsData in this.ReturnsDatas)
                    {
                        foreach (FileType fileType in returnsData.FileTypes)
                        {
                            foreach (ReturnOperation returnOperation in fileType.Operations)
                            {
                                if (returnOperation.Value)
                                {
                                    isReturnOperationSelected = true;
                                    break;
                                }
                            }
                            if (isReturnOperationSelected)
                                break;
                        }
                        if (isReturnOperationSelected)
                            break;
                    }
                    if (!isReturnOperationSelected)
                    {
                        /// TODO: MessageBoxHelper.Show("No operation selected. Select atleast one operation from Available Returns!", "Error");
                        return;
                    }

                    foreach (YearData year in this.ReturnPeriods)
                    {
                        foreach (MonthData month in year.Months)
                        {
                            if (month.IsChecked)
                            {
                                KeepAlive_Task(URLs.DashboardURL);
                                RoleStatus roleStatus = GetRoleStatus_Task(month.Value).Result;
                                if (roleStatus != null && roleStatus.status == 1 && roleStatus.data != null && roleStatus.data.user != null && roleStatus.data.user.Count > 0)
                                {
                                    foreach (User user in roleStatus.data.user)
                                    {
                                        if (user.returns != null && user.returns.Count > 0)
                                        {
                                            foreach (ReturnsData returns in this.ReturnsDatas)
                                            {
                                                Return returnStatus = user.returns.Find(item => item.return_ty == returns.ReturnName.Replace(" ", ""));
                                                if (returnStatus != null)
                                                {
                                                    if (returnStatus.tileDisable == false)
                                                    {
                                                        foreach (FileType fileType in returns.FileTypes)
                                                        {
                                                            if ((!fileType.CheckFiledStatus || returnStatus.status == "FIL" || (fileType.SubmittedIsEnough && returnStatus.status == "FRZ")))
                                                            {
                                                                foreach (ReturnOperation operation in fileType.Operations)
                                                                {
                                                                    if (operation.Value)
                                                                    {
                                                                        if (operation.Action != null)
                                                                        {
                                                                            this.Status = string.Format("Processing {0} {1} Request for {2}...", returns.ReturnName, operation.OperationName, month.Value);
                                                                            CommandResult result = operation.Action(Client, month.Value);
                                                                            if (result.Result == CommandResult.Results.Success)
                                                                            {
                                                                                if (result.Data is List<string>)
                                                                                {
                                                                                    foreach (string url in ((List<string>)result.Data))
                                                                                    {
                                                                                        App.Current.Dispatcher.InvokeAsync(() =>
                                                                                        {
                                                                                            DownloadManager.DownloadItem downloadItem = new DownloadManager.DownloadItem(url, this.DownloadsFolder, string.Format("{0}_{1}_{2}", returns.ReturnName.Replace(" ", ""), month.Value, fileType.FileTypeName), true)
                                                                                            {
                                                                                                CustomCookies = Client.CookieContainer.GetCookies(Client.BaseUrl)
                                                                                            };
                                                                                            downloadManager.Downloads.Add(downloadItem);
                                                                                            App.Current.Dispatcher.Invoke(() => downloadItem.Start.Execute(null));
                                                                                        });
                                                                                    }
                                                                                }
                                                                                else if (result.Data is string)
                                                                                {
                                                                                    if (returns.ReturnName == "GSTR 1")
                                                                                    {
                                                                                        pdfMaker.GenerateGSTR1(result.Data.ToString(), month.Value, returnStatus.status).Wait();
                                                                                    }
                                                                                }
                                                                                else if (result.Data is ReturnDataGSTR3B data)
                                                                                {
                                                                                    pdfMaker.GenerateGSTR3B(data.formDetailsContent, data.summaryContent, data.taxPayableContent, month.Value, returnStatus.status).Wait();
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Log.Warning("Unable to Request Generate/Download. {0} Not Filed for Month {1}", returns.ReturnName.Replace(" ", ""), month.Value);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Log.Warning("{0} Return Status for Month {1} is Disabled!", returns.ReturnName.Replace(" ", ""), month.Value);
                                                    }
                                                }
                                                else
                                                {
                                                    Log.Verbose("Unable to Find {0} Return Status for Month {1}", returns.ReturnName.Replace(" ", ""), month.Value);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    /// TODO: MessageBoxHelper.Show("Process Completed! View Logs for Details", "Done").Wait();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Process failed! {0}", ex.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        private Task<RoleStatus> GetRoleStatus_Task(string monthValue)
        {
            return Task.Run<RoleStatus>(() =>
            {
                RoleStatus value = null;
                bool isBusyChanged = false;
                try
                {
                    if (this.IsBusy != true)
                    {
                        isBusyChanged = true;
                        this.IsBusy = true;
                    }
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
                        Log.Error("Error on RoleStatus Request for " + monthValue);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Fetch RoleStatus Failed!. " + ex.Message);
                }
                finally
                {
                    if (isBusyChanged)
                        this.IsBusy = false;
                }

                return value;
            });
        }

        public DelegateCommand SelectDownloadsFolder { get; }
        private void SelectDownloadsFolder_Task()
        {
            if (this.openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                this.DownloadsFolder = this.openFolderDialog.SelectedPath;
            }
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
                        CheckFiledStatus = true,
                        SubmittedIsEnough = true,
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Download",
                                Action = DownloadMethods.GSTR1_PDF_DOWNLOAD
                            }
                        }
                    },
                    new FileType() {
                        FileTypeName = "JSON",
                        CheckFiledStatus = true,
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
                                Action = DownloadMethods.GSTR2A_EXCEL_GENERATE
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                                Action = DownloadMethods.GSTR2A_EXCEL_DOWNLOAD
                            }
                        }
                    },
                    new FileType() {
                        FileTypeName = "JSON",
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Generate",
                                Action = DownloadMethods.GSTR2A_JSON_GENERATE
                            },
                            new ReturnOperation() {
                                OperationName = "Download",
                                Action = DownloadMethods.GSTR2A_JSON_DOWNLOAD
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
                        CheckFiledStatus = true,
                        Operations = new ObservableCollection<ReturnOperation>() {
                            new ReturnOperation() {
                                OperationName = "Download",
                                Action = DownloadMethods.GSTR3B_PDF_DOWNLOAD
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
