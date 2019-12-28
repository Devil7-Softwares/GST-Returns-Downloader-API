using Avalonia.Threading;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Collections.ObjectModel;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    class ObservableCollectionSink : ILogEventSink
    {
        #region Variables
        private ObservableCollection<Models.LogEvent> logCollection;
        #endregion

        #region Constructor
        public ObservableCollectionSink(ObservableCollection<Models.LogEvent> logCollection)
        {
            this.logCollection = logCollection;
        }
        #endregion

        #region ILogEventSink Implements
        public void Emit(LogEvent logEvent)
        {
            if (logCollection != null)
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    logCollection.Add(new Models.LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.RenderMessage()));
                }
                else
                {
                    Dispatcher.UIThread.InvokeAsync(() => logCollection.Add(new Models.LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.RenderMessage())));
                }
            }
        }
        #endregion
    }

    public static class SinkExtensions
    {
        public static LoggerConfiguration ObservableCollectionSink(this LoggerSinkConfiguration loggerSinkConfiguration, ObservableCollection<Models.LogEvent> logCollection)
        {
            return loggerSinkConfiguration.Sink(new ObservableCollectionSink(logCollection), LogEventLevel.Verbose);
        }
    }
}
