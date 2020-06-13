namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class CommandResult
    {
        public CommandResult(Results result, string message, object data = null)
        {
            this.Data = data;
            this.Message = message;
            this.Result = result;
        }

        public enum Results
        {
            Success,
            Failed
        }

        public Results Result { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }
}