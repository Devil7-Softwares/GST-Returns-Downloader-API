using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Devil7.Automation.GSTR.Downloader.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public static class MessageBoxHelper
    {
        public static Task Show(string message, string title)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                
                var window = MessageBoxManager.GetMessageBoxStandardWindow(title, message);
                return window.Show();
            }
            else
            {
                return Dispatcher.UIThread.InvokeAsync(() => Show(message, title));
            }
        }

        public static Task Show(CommandResult result, Window parent = null)
        {
            var window = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = (result.Result == CommandResult.Results.Success ? "Done" : "Failed"),
                ContentMessage = result.Message,
                Icon = (result.Result == CommandResult.Results.Success ? Icon.Success : Icon.Error),
                Style = Style.None
            });
            if (parent != null)
            {
                return window.ShowDialog(parent);
            }
            else
            {
                return window.Show();
            }
        }

        public static Task ShowError(CommandResult result, Window parent = null)
        {
            if (result.Result == CommandResult.Results.Failed)
            {
                var window = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Error",
                    ContentMessage = result.Message,
                    Icon = Icon.Error,
                    Style = Style.None
                });
                if (parent != null)
                {
                    return window.ShowDialog(parent);
                }
                else
                {
                    return window.Show();
                }
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}