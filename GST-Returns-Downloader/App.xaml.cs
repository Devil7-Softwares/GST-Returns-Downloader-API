using Avalonia;
using Avalonia.Markup.Xaml;

namespace GST_Returns_Downloader
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}