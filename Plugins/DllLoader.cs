using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReSplit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ReSplit.Plugins.DllLoader;

namespace ReSplit.Plugins
{
    public interface IReSplitHost
    {
        void SetStatus(string text);
        void UpdateIGT(TimeSpan value);
        void StartOrSplit();
        void Reset();
    }
    public class ReSplitHost : IReSplitHost 
    {
        public void Reset()
        {
            CentralControls.ResetRun();
        }

        public void SetStatus(string text)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow.Instance.Lbl_Status.Text = text;
            });
        }

        public void StartOrSplit()
        {
            CentralControls.StartNewAttempt();
        }

        public void UpdateIGT(TimeSpan value)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow.Instance.Lbl_IGT.Text = TimeSpanFormat.FormatNewTime(value);
            });
        }
    }
    public static class DllLoader
    {
        public static async Task LoadAndInitialize(Window owner)
        {
            try
            {

            var files = await owner.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select ReSplit Plugin",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("DLL")
                        {
                            Patterns = new[] { "*.dll" }
                        }
                    }
                });

            if (files.Count == 0)
                return;
            var host = new ReSplitHost();

            string dllPath = files[0].Path.LocalPath;
            Assembly asm = Assembly.LoadFrom(dllPath);

            var pluginType = asm.GetType("ReSplitPlugins.ReSplitPlugin");
            if (pluginType == null)
                throw new Exception("ReSplitPlugin not found");

            var init = pluginType.GetMethod(
                "Initialize",
                BindingFlags.Public | BindingFlags.Static
            );

            if (init == null)
                throw new Exception("Initialize() not found");

            init.Invoke(null, new object[] { host });
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
