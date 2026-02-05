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
        public static SplitsModel CurrentSplit;
        public static TimerState CurrentTimerState = TimerState.NotStarted;
        public static bool IsRunning => CurrentTimerState == TimerState.Running;
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
                CalculatePredictedTime(TimeSpan.Zero);
                GlobalTimer.Start();
            }
        }
        public static void Split()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x=>x.IsActive);
            if (current == null) return;
            // Logic for handling the split
            current.NewTime = GlobalTimer.GetElapsedTime();
            if (current.Time != TimeSpan.Zero)
            {
                var delta = current.NewTime - current.Time;
                current.DeltaTime = delta;
                current.F_DeltaTime = TimeSpanFormat.FormatDelta(delta);
                current.F_Time = TimeSpanFormat.FormatNewTime(current.NewTime);
                if(delta > TimeSpan.Zero)
                    current.DeltaForegroundColor = "Red";
            }
            else
            {
                current.F_DeltaTime = "-";
                current.F_Time = TimeSpanFormat.FormatNewTime(current.NewTime);
            }
            current.IsActive = false;
            var nextIndex = StaticBinding.Splits.IndexOf(current) + 1;
            var nextSplit = StaticBinding.Splits.ElementAtOrDefault(nextIndex);
            if (nextSplit == null)
            {
                // Finished all splits
                UpdateTimerState(TimerState.Ended);
                GlobalTimer.Stop();
            }
            else
            {
                nextSplit.IsActive = true;
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Data_SplitList.ScrollIntoView(nextSplit);
                });
                if(current.F_DeltaTime != "-")
                    CalculatePredictedTime(current.NewTime - current.Time);
            }
        }
        public static void UndoSplit()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x => x.IsActive);
            if (current == null) return;
            // Logic for skipping the current split
            current.NewTime = GlobalTimer.GetElapsedTime();

            current.F_DeltaTime = "";
            current.F_Time = TimeSpanFormat.FormatNewTime(current.Time);
            current.IsActive = false;

            // this next is previous, im just lazy
            var nextIndex = StaticBinding.Splits.IndexOf(current) - 1;
            var nextSplit = StaticBinding.Splits.ElementAtOrDefault(nextIndex);
            if (nextSplit == null)
            {
                // Finished all splits
                UpdateTimerState(TimerState.NotStarted);
                GlobalTimer.Stop();
            }
            else
            {
                nextSplit.F_DeltaTime = "";
                nextSplit.F_Time = TimeSpanFormat.FormatNewTime(nextSplit.Time);
                nextSplit.IsActive = true;
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Data_SplitList.ScrollIntoView(nextSplit);
                });
            }
        }
        public static void SkipSplit()
        {
            var current = StaticBinding.Splits.FirstOrDefault(x => x.IsActive);
            if (current == null) return;
            // Logic for skipping the current split
            current.NewTime = GlobalTimer.GetElapsedTime();

            current.F_DeltaTime = "";
            current.F_Time = "-";
            current.IsActive = false;

            var nextIndex = StaticBinding.Splits.IndexOf(current) + 1;
            var nextSplit = StaticBinding.Splits.ElementAtOrDefault(nextIndex);
            if (nextSplit == null)
            {
                // Finished all splits
                UpdateTimerState(TimerState.Ended);
                GlobalTimer.Stop();
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
        public static void ResetRun()
        {
            UpdateTimerState(TimerState.NotStarted);
            GlobalTimer.Reset();
            // Additional logic for resetting the run
            foreach(var split in StaticBinding.Splits)
            {
                split.NewTime = TimeSpan.Zero;
                split.F_DeltaTime = "";
                split.F_Time = TimeSpanFormat.FormatNewTime(split.Time);
                split.IsActive = false;
            }
            if(StaticBinding.Splits.Count > 0)
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
        public static void UpdateTimerState(TimerState state)
        {
            CurrentTimerState = state;
            if(state == TimerState.NotStarted)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
                });
            }
            if(state == TimerState.Ended)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF3B82F6"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF3B82F6"));
                });
            }
            if(state == TimerState.Running)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF00CC36"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF00CC36"));
                });
            }
            if(state == TimerState.Paused)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF7A7A7A"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF7A7A7A"));
                });
            }
            if(state == TimerState.LosingTime)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FFFF0000"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FFFF0000"));
                });
            }
            if(state == TimerState.GainingTime)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MainWindow.Instance.Lbl_Timer.Foreground = new SolidColorBrush(Color.Parse("#FF00FF00"));
                    MainWindow.Instance.Lbl_Milliseconds.Foreground = new SolidColorBrush(Color.Parse("#FF00FF00"));
                });
            }
        }
        public static void CalculatePredictedTime(TimeSpan delta)
        {
            var lastCompletedSplit = StaticBinding.Splits
                .LastOrDefault(s => s.Time > TimeSpan.Zero);

            if (lastCompletedSplit == null)
                return;

            var predictedTimeSpan = lastCompletedSplit.Time + delta;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainWindow.Instance.Lbl_Prediction.Text =
                    TimeSpanFormat.FormatNewTime(predictedTimeSpan);
            });
        }

        public static void CalculateBestPossibleTime()
        {
            // TODO
            var bestPossibleTime = TimeSpan.Zero;
            var listtime = new List<TimeSpan>();

            TimeSpan? lowest = null;
            // add from run
            foreach (var segment in StaticBinding.CurrentRun.Segments)
            {
                foreach (var splitTime in segment.SplitTimes)
                {
                    if (!string.IsNullOrWhiteSpace(splitTime.RealTime) &&
                        TimeSpan.TryParse(splitTime.RealTime, out var time))
                    {
                        if (lowest == null || time < lowest)
                            lowest = time;
                    }
                }
                if (lowest.HasValue)
                    listtime.Add(lowest.Value);
            }
        }
    }
}
