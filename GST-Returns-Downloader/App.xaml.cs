using Avalonia;
using Avalonia.Markup.Xaml;

namespace Devil7.Automation.GSTR.Downloader
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}