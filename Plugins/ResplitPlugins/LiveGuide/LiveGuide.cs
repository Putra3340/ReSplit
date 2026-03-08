using Avalonia.Controls;
using Avalonia.Threading;
using ReSplit.Plugins;
using System.Collections.ObjectModel;
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
        public static void Initialize(IReSplitHost host)
        {
            Host = host;
            DllPath = host.IdentifierPath;

            // You can perform any necessary initialization here, such as setting up event handlers or preparing resources.
            // For example, this plugin gonna create a simple window that displays the current splits and allows the user to interact with them.
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
            });
            Host?.Shutdown(DllPath);
        }
    }
}
