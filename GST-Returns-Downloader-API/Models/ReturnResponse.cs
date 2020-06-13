using System.Collections.Generic;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class ReturnResponseData
    {
        public int status { get; set; }
        public string msg { get; set; }
        public string token { get; set; }
        public List<string> url { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string timeStamp { get; set; }
        public int rc { get; set; }
    }

    public class ReturnResponseError
    {
        public string errorCode { get; set; }
        public string message { get; set; }
        public string detailMessage { get; set; }
        public List<object> stackTrace { get; set; }
        public List<object> suppressedExceptions { get; set; }
    }

    public class ReturnResponse
    {
        public int status { get; set; }
        public ReturnResponseError error { get; set; }
        public ReturnResponseData data { get; set; }
    }
}
