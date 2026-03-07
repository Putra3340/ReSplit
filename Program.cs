using Avalonia;
using Avalonia.WebView.Desktop;
using AvaloniaWebView;
using System;

namespace ReSplit
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
            .AfterSetup(_ =>
            {
                AvaloniaWebViewBuilder.Initialize(config =>
                {
                    config.UserDataFolder = "webview-data";
                    config.AreDevToolEnabled = true;
                });
            })
            .UseDesktopWebView()
                .LogToTrace();
    }
}
