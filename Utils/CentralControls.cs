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
        public static bool IsRunning { get; set; } = false;
        public static void StartNewAttempt()
        {
            if (IsRunning)
            {
                Split();
                return;
            }
            else
            {
                IsRunning = true;
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
                IsRunning = false;
                GlobalTimer.Stop();
            }
            else
            {
                nextSplit.IsActive = true;
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
                IsRunning = false;
                GlobalTimer.Stop();
            }
            else
            {
                nextSplit.F_DeltaTime = "";
                nextSplit.F_Time = TimeSpanFormat.FormatNewTime(nextSplit.Time);
                nextSplit.IsActive = true;
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
                IsRunning = false;
                GlobalTimer.Stop();
            }
            else
            {
                nextSplit.IsActive = true;
            }
        }
        public static void ResetRun()
        {
            IsRunning = false;
            GlobalTimer.Reset();
            // Additional logic for resetting the run
        }
        public static void Pause()
        {
            GlobalTimer.Stop();
            // Additional logic for pausing the run
        }
        
    }
}
