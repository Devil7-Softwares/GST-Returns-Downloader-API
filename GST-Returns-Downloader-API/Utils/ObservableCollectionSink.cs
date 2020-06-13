using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace Devil7.Automation.GSTR.Downloader.Utils
{
    class ObservableCollectionSink : ILogEventSink
    {
        #region Variables
        private object syncLock;
        private ObservableCollection<Models.LogEvent> logCollection;

        #endregion

        #region Constructor
        public ObservableCollectionSink(ObservableCollection<Models.LogEvent> logCollection,ref object syncLock)
        {
            this.logCollection = logCollection;
            this.syncLock = syncLock;
        }
        #endregion

        #region ILogEventSink Implements
        public void Emit(LogEvent logEvent)
        {
            if (logCollection != null)
            {
                lock (syncLock)
                {
                    logCollection.Add(new Models.LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.RenderMessage()));
                }
            }
        }
        #endregion
    }

    public static class SinkExtensions
    {
        public static LoggerConfiguration ObservableCollectionSink(this LoggerSinkConfiguration loggerSinkConfiguration, ObservableCollection<Models.LogEvent> logCollection, ref object syncLock)
        {
            return loggerSinkConfiguration.Sink(new ObservableCollectionSink(logCollection, ref syncLock), LogEventLevel.Verbose);
        }
    }
}
