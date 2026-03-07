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
            if(mi.Header.ToString() == "Split") CentralControls.StartNewAttempt();
            if(mi.Header.ToString() == "Create Segment")
            {
                CreateSegmentWindow cs = new();
                cs.ShowDialog(this);
            }
            if(mi.Header.ToString() == "Reset") CentralControls.ResetRun();
            if(mi.Header.ToString() == "Pause") CentralControls.Pause();
            if(mi.Header.ToString() == "Skip") CentralControls.SkipSplit();
            if(mi.Header.ToString() == "Undo") CentralControls.UndoSplit();
            if(mi.Header.ToString() == "Exit") this.Close();
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
                SetupLoad(path);
            }
        }
        private void SetupLoad(string splitPath)
        {
            StaticBinding.CurrentRun = RunSerializer.Load(splitPath);
            StaticBinding.Splits.Clear();
            Lbl_Title.Text = StaticBinding.CurrentRun.GameName ?? "Untitled Run";
            Lbl_Category.Text = StaticBinding.CurrentRun.CategoryName ?? "No Category";
            Lbl_Platform.Text = StaticBinding.CurrentRun.Platform ?? "No Platform";
            foreach (var split in StaticBinding.CurrentRun.Segments)
            {
                TimeSpan t = TimeSpan.TryParse(split.SplitTimes.Last().RealTime,out var a) ? a : TimeSpan.Zero;
                string time = t.TotalHours >= 1
                ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}"
                    : t.TotalMinutes >= 1
                        ? $"{t.Minutes:00}:{t.Seconds:00}"
                            : t.TotalSeconds < 1
                                ? "-"
                                    : $"{t.Seconds}";

                StaticBinding.Splits.Add(new SplitsModel { F_Name = split.Name, F_Time = time, Time = t});
            }
            StaticBinding.Splits[0].IsActive = true;
        }
    }
}