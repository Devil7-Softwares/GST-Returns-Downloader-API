using System;
using System.Collections.Generic;
using System.Text;

namespace Devil7.Automation.GSTR.Downloader.Misc {
    class URLs {
        #region BaseURLs
        public const string ServicesURL = "https://services.gst.gov.in/";
        public const string ReturnsURL = "https://return.gst.gov.in/";
        #endregion

        #region Nodes
        public const string Login = "/services/login";
        public const string Captcha = "/services/captcha";
        public const string Authendicate = "/services/authenticate";
        public const string Auth = "/services/auth/";
        public const string KeepAlive = "/{0}/auth/api/keepalive";
        #endregion

        #region FullURLs
        public const string WelcomeURL = "https://services.gst.gov.in/services/auth/fowelcome";
        public const string LoginURL = "https://services.gst.gov.in/services/login";
        #endregion
    }
}