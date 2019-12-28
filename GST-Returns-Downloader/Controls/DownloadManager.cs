using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Styling;
using ReactiveUI;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader.Controls
{
    public class DownloadManager : ItemsControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(DownloadManager);

        #region Variables
        public static string UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36";
        #endregion

        #region Properties
        public ObservableCollection<DownloadItem> Downloads
        {
            get
            {
                if (this.Items == null || !(this.Items is ObservableCollection<DownloadItem>))
                {
                    this.Items = new ObservableCollection<DownloadItem>();
                }
                return this.Items as ObservableCollection<DownloadItem>;
            }
        }
        #endregion

        #region DownloadItem
        public class DownloadItem : ReactiveObject
        {
            #region Constructor
            public DownloadItem(string URL, string Path, string FileName = "")
            {
                this.URL = URL;
                this.Path = Path;
                this.FileName = FileName;

                this.Start = ReactiveCommand.CreateFromTask(start);
                this.CustomHeaders = new Dictionary<string, string>();

                Log.Verbose("Creating Download Item URL: {0}, Path: {1}, FileName: {3}", url, path, FileName);
            }
            #endregion

            #region Variables
            private DateTime statusLastUpdated = DateTime.Now;
            private long sizeWhenLastUpdated = 0;
            #endregion

            #region Properties
            private string url;
            public string URL
            {
                get => url;
                set => this.RaiseAndSetIfChanged(ref url, value);
            }

            private string path;
            public string Path
            {
                get => path;
                set => this.RaiseAndSetIfChanged(ref path, value);
            }

            private string fileName;
            public string FileName
            {
                get => fileName;
                private set => this.RaiseAndSetIfChanged(ref fileName, value);
            }

            private long totalSize;
            public long TotalSize
            {
                get => totalSize;
                private set => this.RaiseAndSetIfChanged(ref totalSize, value);
            }

            private long downloadedSize;
            public long DownloadedSize
            {
                get => downloadedSize;
                private set => this.RaiseAndSetIfChanged(ref downloadedSize, value);
            }

            private long progress = 0;
            public long Progress
            {
                get => progress;
                private set => this.RaiseAndSetIfChanged(ref progress, value);
            }

            private string status = "Ready";
            public string Status
            {
                get => status;
                private set => this.RaiseAndSetIfChanged(ref status, value);
            }

            public CookieCollection CustomCookies { get; set; }

            public Dictionary<string, string> CustomHeaders { get; set; }
            #endregion

            #region Methods
            private void UpdateStatus()
            {
                string Speed = "0 MB";
                
                if (this.TotalSize > 0)
                    this.Progress = (int) Math.Round(((decimal)this.DownloadedSize / (decimal)this.TotalSize) * 100m, 0);
                else
                    this.Progress = 0;

                #region Speed Calculation
                long currentSize = DownloadedSize;
                DateTime currentTime = DateTime.Now;

                long sizeDiff = currentSize - sizeWhenLastUpdated;
                TimeSpan timeDiff = currentTime.Subtract(statusLastUpdated);

                long speedInBytes = 1;

                if (timeDiff.TotalSeconds > 0)
                {
                    speedInBytes = (long)Math.Round(sizeDiff / timeDiff.TotalSeconds, 0);
                    if (speedInBytes <= 0) speedInBytes = 1;
                    Speed = BytesToString(speedInBytes);
                }

                statusLastUpdated = currentTime;
                sizeWhenLastUpdated = currentSize;
                #endregion

                if (TotalSize < 0)
                {
                    this.Status = string.Format("{0} downloaded. ({1}/sec)", BytesToString(DownloadedSize), Speed);
                }
                else if (DownloadedSize >= TotalSize)
                {
                    this.Status = "Download completed.";
                }
                else
                {
                    string TimeRemaining = "0m";

                    #region Time Remaining Calculation
                    long remainingSeconds = (TotalSize - DownloadedSize) / speedInBytes;
                    TimeRemaining = TimeSpanToString(TimeSpan.FromSeconds(remainingSeconds));
                    #endregion

                    this.Status = string.Format("{0} left - {1} of {2} ({3}/sec)", TimeRemaining, BytesToString(DownloadedSize), BytesToString(TotalSize), Speed);
                }
            }

            private string TimeSpanToString(TimeSpan timeSpan)
            {
                string timeSpanStr = "";
                if (timeSpan.Hours > 0)
                    timeSpanStr += timeSpan.Hours + "h ";
                if (timeSpan.Minutes > 0)
                    timeSpanStr += timeSpan.Minutes + "m";
                else
                {
                    timeSpanStr += timeSpan.Seconds + "s";
                }
                return timeSpanStr.Trim();
            }

            private string BytesToString(long byteCount)
            {
                string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
                if (byteCount == 0)
                    return "0" + suf[0];
                long bytes = Math.Abs(byteCount);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return (Math.Sign(byteCount) * num).ToString() + suf[place];
            }
            #endregion

            #region Commands
            public ReactiveCommand<Unit, Unit> Start
            {
                get;
            }
            private Task start()
            {
                return Task.Run(() =>
                {
                    Timer statusUpdater = null;
                    Uri downloadURL = new Uri(this.URL);

                    try
                    {
                        // Create a WebRequest object and assign it a cookie container and make them think your Mozilla ;)
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.URL);
                        webRequest.Method = "GET";
                        webRequest.Accept = "*/*";
                        webRequest.AllowAutoRedirect = false;
                        webRequest.UserAgent = UserAgent;
                        webRequest.CookieContainer = new CookieContainer();

                        if (this.CustomCookies != null) webRequest.CookieContainer.Add(this.CustomCookies);

                        // Grab the response from the server for the current WebRequest
                        HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

                        if (!String.IsNullOrEmpty(webResponse.Headers["Content-Disposition"]))
                        {
                            this.FileName = webResponse.Headers["Content-Disposition"].Substring(webResponse.Headers["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                        }
                        if (String.IsNullOrEmpty(fileName))
                        {
                            this.FileName = System.IO.Path.GetFileName(URL);
                        }

                        TotalSize = webResponse.ContentLength;

                        this.statusLastUpdated = DateTime.Now;
                        this.sizeWhenLastUpdated = 0;

                        using (FileStream fs = new FileStream(System.IO.Path.Combine(this.Path, this.FileName), FileMode.Create))
                        {
                            statusUpdater = new Timer((object state) => { UpdateStatus(); }, null, 0, 1000);
                            using (Stream responseStream = webResponse.GetResponseStream())
                            {
                                byte[] buffer = new byte[0x1000];
                                int bytes;
                                int offset = 0;
                                while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fs.Write(buffer, 0, bytes);
                                    offset += bytes + 1;
                                    DownloadedSize = offset;
                                }
                            }
                            statusUpdater.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error on downloading. File: '{2}', URL: '{0}', Path: '{1}'", URL, Path, FileName);
                    }
                    finally
                    {
                        if (statusUpdater != null) statusUpdater.Dispose();
                        UpdateStatus();
                    }
                });
            }
            #endregion

        }
        #endregion
    }
}
