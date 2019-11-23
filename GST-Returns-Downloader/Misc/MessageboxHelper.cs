using System.Threading.Tasks;
using Avalonia.Controls;
using Devil7.Automation.GSTR.Downloader.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace Devil7.Automation.GSTR.Downloader.Misc {
    public static class MessageBoxHelper {
        public static Task Show (string message, string title) {
            var window = new MessageBoxWindow (title, message);
            return window.Show ();
        }

        public static Task Show (CommandResult result, Window parent = null) {
            var window = new MessageBoxWindow (new MessageBoxParams {
                Button = ButtonEnum.Ok,
                    ContentTitle = (result.Result == CommandResult.Results.Success ? "Done" : "Failed"),
                    ContentMessage = result.Message,
                    Icon = (result.Result == CommandResult.Results.Success ? Icon.Success : Icon.Error),
                    Style = Style.None
            });
            if (parent != null) {
                return window.ShowDialog (parent);
            } else {
                return window.Show ();
            }
        }

        public static Task ShowError (CommandResult result, Window parent = null) {
            if (result.Result == CommandResult.Results.Failed) {
                var window = new MessageBoxWindow (new MessageBoxParams {
                    Button = ButtonEnum.Ok,
                        ContentTitle = "Error",
                        ContentMessage = result.Message,
                        Icon = Icon.Error,
                        Style = Style.None
                });
                if (parent != null) {
                    return window.ShowDialog (parent);
                } else {
                    return window.Show ();
                }
            } else {
                return Task.FromResult(false);
            }
        }
    }
}