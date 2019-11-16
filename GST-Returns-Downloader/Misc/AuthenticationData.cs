using Newtonsoft.Json;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public class AuthenticationData
    {
        public AuthenticationData(string username, string password, string captcha)
        {
            this.username = username;
            this.password = password;
            this.captcha = captcha;

            this.deviceID = null;
            this.type = "username";

            this.mFP = JsonConvert.SerializeObject(new DeviceDNA());
        }

        public string username { get; set; }
        public string password { get; set; }
        public string captcha { get; set; }
        public string mFP { get; set; }
        public object deviceID { get; set; }
        public string type { get; set; }

        public class Browser
        {
            public Browser()
            {
                this.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36";
                this.Vendor = "Google Inc.";
                this.VendorSubID = "";
                this.BuildID = "20030107";
                this.CookieEnabled = true;
            }
            public string UserAgent { get; set; }
            public string Vendor { get; set; }
            public string VendorSubID { get; set; }
            public string BuildID { get; set; }
            public bool CookieEnabled { get; set; }
        }

        public class IEPlugins
        {
        }

        public class NetscapePlugins
        {
            [JsonProperty(PropertyName = "Chrome PDF Plugin")]
            public string ChromePDFPlugin { get; set; }
            [JsonProperty(PropertyName = "Chrome PDF Viewer")]
            public string ChromePDFViewer { get; set; }
            [JsonProperty(PropertyName = "Native Client")]
            public string NativeClient { get; set; }
        }

        public class Screen
        {
            public Screen()
            {
                this.FullHeight = 900;
                this.AvlHeight = 872;
                this.FullWidth = 1600;
                this.AvlWidth = 1600;
                this.ColorDepth = 24;
                this.PixelDepth = 24;
            }
            public int FullHeight { get; set; }
            public int AvlHeight { get; set; }
            public int FullWidth { get; set; }
            public int AvlWidth { get; set; }
            public int ColorDepth { get; set; }
            public int PixelDepth { get; set; }
        }

        public class System
        {
            public System()
            {
                this.Platform = "Linux x86_64";
                this.systemLanguage = "en-IN";
                this.Timezone = -330;
            }
            public string Platform { get; set; }
            public string systemLanguage { get; set; }
            public int Timezone { get; set; }
        }

        public class MFP
        {
            public MFP()
            {
                this.Browser = new Browser();
                this.IEPlugins = new IEPlugins();
                this.NetscapePlugins = new NetscapePlugins();
                this.Screen = new Screen();
                this.System = new System();
            }
            public Browser Browser { get; set; }
            public IEPlugins IEPlugins { get; set; }
            public NetscapePlugins NetscapePlugins { get; set; }
            public Screen Screen { get; set; }
            public System System { get; set; }
        }

        public class MESC
        {
            public MESC()
            {
                this.mesc = Misc.MESC.getMESC();
            }
            public string mesc { get; set; }
        }

        public class DeviceDNA
        {
            public DeviceDNA()
            {
                this.VERSION = "2.1";
                this.ExternalIP = "";
                this.MESC = new MESC();
                this.MFP = new MFP();
            }
            public string VERSION { get; set; }
            public MFP MFP { get; set; }
            public string ExternalIP { get; set; }
            public MESC MESC { get; set; }
        }
    }
}