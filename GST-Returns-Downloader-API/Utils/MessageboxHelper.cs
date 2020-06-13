using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using Devil7.Automation.GSTR.Downloader.Models;


namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public class MessageBoxHelper
    {
        public MessageBoxHelper(DXMessageBoxService messageBoxService)
        {
            this.MessageBoxService = messageBoxService;
        }
        public DXMessageBoxService MessageBoxService { get; }
        public void Show(string message, string title)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                this.MessageBoxService.ShowMessage(message, title, MessageButton.OK, MessageIcon.Information);
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(() => Show(message, title));
            }
        }

        public void Show(CommandResult result, Window parent = null)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                this.MessageBoxService.ShowMessage(result.Message, result.Result == CommandResult.Results.Success ? "Done" : "Failed", MessageButton.OK, result.Result == CommandResult.Results.Success ? MessageIcon.Information : MessageIcon.Error);
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(() => Show(result, parent));
            }
        }
    }
}