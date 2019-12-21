namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class AuthResponse
    {
        public object url { get; set; }
        public string message { get; set; }
        public string successCode { get; set; }
        public string errorCode { get; set; }
    }
}