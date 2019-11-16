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
        #endregion
    }
}