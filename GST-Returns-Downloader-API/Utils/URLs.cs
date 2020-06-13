using System;
using System.Collections.Generic;
using System.Text;

namespace Devil7.Automation.GSTR.Downloader.Utils
{
    class URLs
    {
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
        public const string GstrReturnGenerateOrDownload = "/returns/auth/api/offline/download/generate?{0}flag={1}&rtn_prd={2}&rtn_typ={3}";
        public const string Gstr1Data = "/returns/auth/api/gstr1/summary?rtn_prd={0}";
        public const string UserRegDetails = "/returns/auth/api/gstr1/userdetails?ctin={0}";
        public const string Gstr3BFormDetails = "/returns/auth/api/formdetails?rtn_prd={0}&rtn_typ=GSTR3B";
        public const string Gstr3BSummary = "/returns/auth/api/gstr3b/summary?rtn_prd={0}";
        public const string Gstr3BTaxpayable = "/returns/auth/api/gstr3b/taxpayble?rtn_prd={0}";
        #endregion

        #region FullURLs
        public const string WelcomeURL = "https://services.gst.gov.in/services/auth/fowelcome";
        public const string LoginURL = "https://services.gst.gov.in/services/login";
        public const string DashboardURL = "https://return.gst.gov.in/returns/auth/dashboard";
        public const string GstrOfflineDownloadURL = "https://return.gst.gov.in/returns/auth/gstr/offlinedownload";
        public const string Gstr1URL = "https://return.gst.gov.in/returns/auth/gstr1";
        #endregion
    }
}