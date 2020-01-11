using Serilog.Events;
using System;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class LogEvent
    {
        #region Constructor
        public LogEvent(DateTimeOffset Time, LogEventLevel Level, string Message)
        {
            this.Time = Time.ToString();
            this.Message = Message;
            this.Level = Level;
        }
        #endregion

        #region Properties
        public String Time { get; }
        public LogEventLevel Level { get; }
        public string Message { get; }
        #endregion
    }
}
