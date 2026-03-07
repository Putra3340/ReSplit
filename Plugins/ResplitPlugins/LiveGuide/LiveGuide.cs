using Avalonia.Controls;
using Avalonia.Threading;
using ReSplit.Plugins;
using System.Reflection;

namespace LiveGuide
{
    public class LiveGuidePlugin
    {
        public static IReSplitHost? Host;
        public static string Name => "Live Guide";
        public static string Description => "Live Guide Plugin";
        public static string DllPath = string.Empty;
        public static MainWindow? MainWindow;
        public static SettingsWindow? SettingsWindow;
        public static void Initialize(IReSplitHost host)
        {
            Host = host;
            DllPath = host.DllPath;
            Dispatcher.UIThread.Post(() =>
            {
                if (MainWindow == null)
                {
                    MainWindow = new MainWindow();
                    MainWindow.Closed += (s, e) =>
                    {
                        MainWindow = null;
                        Shutdown();
                    };
                }
                else
                {
                    MainWindow.Activate();
                }
                MainWindow.Show();
            });
        }
        public static void Shutdown()
        {
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow?.Close();
                SettingsWindow?.Close();
            });
            Host?.Shutdown(DllPath);
        }
    }
}
