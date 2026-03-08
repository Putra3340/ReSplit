using Avalonia.Media;
using Avalonia.Threading;
using ReSplit.Models;
using ReSplit.Models.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReSplit.Utils
{
    public static class CentralControls
    {
        public static TimerState CurrentTimerState = TimerState.NotStarted;
        public static bool IsRunning => CurrentTimerState == TimerState.Running;
        public static string CurrentSplitPath = string.Empty;
        public static void StartNewAttempt()
        {
            if (IsRunning)
            {
                Split();
                return;
            }
            else
            {
                UpdateTimerState(TimerState.Running);
                GlobalTimer.Start();
            }
        }
        public static void Split()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x => x.IsActive);
            if (current == null) return;
            if (current == null) return;
            current.NewTime = GlobalTimer.GetElapsedTime();

            // Calculate Delta Time if this is not the first split
            if (current.Time != TimeSpan.MinValue)
            {
                var delta = current.NewTime - current.Time;
                current.DeltaTime = delta;
            }
            else
                current.DeltaTime = TimeSpan.MinValue;
            current.IsActive = false;
            var nextIndex = StaticBinding.Splits.IndexOf(current) + 1;
            var nextSplit = StaticBinding.Splits.ElementAtOrDefault(nextIndex);
            if (nextSplit == null) // Finish
            {
                Finish();
            }
            else
            {
                nextSplit.IsActive = true;
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Data_SplitList.ScrollIntoView(nextSplit);
                });
            }
        }
        public static void UndoSplit()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x => x.IsActive);
            if (current == null) return;
            var prevIndex = StaticBinding.Splits.IndexOf(current) - 1;
            var prevSplit = StaticBinding.Splits.ElementAtOrDefault(prevIndex);
            if (prevSplit == null) // do nothing, we are at the start
                return;

            current.NewTime = GlobalTimer.GetElapsedTime();

            current.DeltaTime = TimeSpan.MaxValue;
            current.NewTime = current.Time;
            current.IsActive = false;

            prevSplit.DeltaTime = TimeSpan.MaxValue;
            prevSplit.NewTime = prevSplit.Time;
            prevSplit.IsActive = true;
            Dispatcher.UIThread.Invoke(() =>
            {
                MainWindow.Instance.Data_SplitList.ScrollIntoView(prevSplit);
            });
        }
        public static void SkipSplit()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x => x.IsActive);
            if (current == null) return;
            var nextIndex = StaticBinding.Splits.IndexOf(current) + 1;
            var nextSplit = StaticBinding.Splits.ElementAtOrDefault(nextIndex);
            if (nextSplit == null)
                return;
            current.NewTime = TimeSpan.MinValue;

            current.DeltaTime = TimeSpan.MaxValue;
            current.IsActive = false;

            nextSplit.IsActive = true;
            Dispatcher.UIThread.Invoke(() =>
            {
                MainWindow.Instance.Data_SplitList.ScrollIntoView(nextSplit);
            });
        }
        public static void ResetRun()
        {
            UpdateTimerState(TimerState.NotStarted);
            GlobalTimer.Reset();
            // Additional logic for resetting the run
            foreach (var split in StaticBinding.Splits)
            {
                split.NewTime = split.Time;
                split.DeltaTime = TimeSpan.MaxValue;
                split.IsActive = false;
            }
            if (StaticBinding.Splits.Count > 0)
                StaticBinding.Splits[0].IsActive = true;
            Dispatcher.UIThread.Invoke(() =>
            {
                MainWindow.Instance.Data_SplitList.ScrollIntoView(0);
            });
        }
        public static void Pause()
        {
            UpdateTimerState(TimerState.Paused);
            GlobalTimer.Stop();
        }
        public static void Finish()
        {
            UpdateTimerState(TimerState.Ended);
            GlobalTimer.Stop();
            Save(CurrentSplitPath);
        }
        public static void SetupLoad(string splitPath)
        {
            CurrentSplitPath = splitPath;
            Dispatcher.UIThread.Post(() =>
            {
                StaticBinding.CurrentRun = RunSerializer.Load(splitPath);
                StaticBinding.Splits.Clear();
                MainWindow.Instance.Lbl_Title.Text = StaticBinding.CurrentRun.GameName ?? "Untitled Run";
                MainWindow.Instance.Lbl_Category.Text = StaticBinding.CurrentRun.CategoryName ?? "No Category";
                MainWindow.Instance.Lbl_Platform.Text = StaticBinding.CurrentRun.Platform ?? "No Platform";
                foreach (var split in StaticBinding.CurrentRun.Segments)
                {
                    TimeSpan t = TimeSpan.TryParse(split.SplitTimes.Last().RealTime, out var a) ? a : TimeSpan.MinValue;
                    string time = TimeSpanFormat.FormatNewTime(t);
                    string id = split.Id ?? Guid.NewGuid().ToString();
                    StaticBinding.Splits.Add(new SplitsModel { Id = id, Name = split.Name, Time = t });
                }
                StaticBinding.Splits[0].IsActive = true;
            });
        }
        public static void Save(string splitPath)
        {
            // Todo: Save the run with the new times
        }
        public static void UpdateTimerState(TimerState state)
        {
            CurrentTimerState = state;
            if (state == TimerState.NotStarted)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
                });
            }
            if (state == TimerState.Ended)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF3B82F6"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF3B82F6"));
                });
            }
            if (state == TimerState.Running)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF00CC36"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF00CC36"));
                });
            }
            if (state == TimerState.Paused)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF7A7A7A"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF7A7A7A"));
                });
            }
            if (state == TimerState.LosingTime)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FFFF0000"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FFFF0000"));
                });
            }
            if (state == TimerState.GainingTime)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF00FF00"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF00FF00"));
                });
            }
        }
    }
}
