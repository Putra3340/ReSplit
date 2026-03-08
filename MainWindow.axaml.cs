using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReSplit.Models;
using ReSplit.Models.Form;
using ReSplit.Plugins;
using ReSplit.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
namespace ReSplit
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static GlobalHotkeyService Hotkeys;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            GlobalTimer.Init();
            Hotkeys = new GlobalHotkeyService();

            Data_SplitList.ItemsSource = StaticBinding.Splits;
        }

        private void DragWindow(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            // LEFT click
            if (point.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
                return;
            }

        }

        private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is not MenuItem mi) return;
            if (mi.Header.ToString() == "Load") OpenRunFileAsync();
            if (mi.Header.ToString() == "Split") CentralControls.StartNewAttempt();
            if (mi.Header.ToString() == "Create Segment")
            {
                CreateSegmentWindow cs = new();
                cs.ShowDialog(this);
            }
            if (mi.Header.ToString() == "Reset") CentralControls.ResetRun();
            if (mi.Header.ToString() == "Pause") CentralControls.Pause();
            if (mi.Header.ToString() == "Skip") CentralControls.SkipSplit();
            if (mi.Header.ToString() == "Undo") CentralControls.UndoSplit();
            if (mi.Header.ToString() == "Exit") this.Close();
            if (mi.Header.ToString() == "Load DLL") PluginLoader.LoadAndInitialize(this);
        }

        private async Task OpenRunFileAsync()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open Run File",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                new FilePickerFileType("LiveSplit Run")
                {
                    Patterns = new[] { "*.lss" }
                }
                    }
                });

            if (files.Count > 0)
            {
                var path = files[0].Path.LocalPath;
                CentralControls.SetupLoad(path);
            }
        }
        
    }
}