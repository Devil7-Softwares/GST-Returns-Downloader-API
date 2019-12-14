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
        public const string Months = "/returns/auth/api/dropdown";
        public const string UserStatus = "/services/api/ustatus";
        public const string RoleStatus = "/returns/auth/api/rolestatus?rtn_prd={0}";
        public const string Gstr1JsonGenerateForce = "/returns/auth/api/offline/download/generate?flag=1&rtn_prd={0}&rtn_typ=GSTR1";
        public const string Gstr1JsonGenerateOrDownload = "/returns/auth/api/offline/download/generate?flag=0&rtn_prd={0}&rtn_typ=GSTR1";
        #endregion

        #region FullURLs
        public const string WelcomeURL = "https://services.gst.gov.in/services/auth/fowelcome";
        public const string LoginURL = "https://services.gst.gov.in/services/login";
        public const string DashboardURL = "https://return.gst.gov.in/returns/auth/dashboard";
        public const string GstrOfflineDownloadURL = "https://return.gst.gov.in/returns/auth/gstr/offlinedownload";
        #endregion
    }
}