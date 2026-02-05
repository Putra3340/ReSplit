using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;
using Avalonia.Threading;


namespace ReSplit
{
    // this is the timer lives but it update on TimerModel.cs
    public static class GlobalTimer
    {
        private static Stopwatch _stopwatch = new();
        private static Timer _timer;

        public static void Init()
        {
            _timer = new Timer(16); // ~60 FPS
            _timer.Elapsed += (_, _) =>
            {
                Dispatcher.UIThread.Post(UpdateTime);
            };
        }

        private static void UpdateTime()
        {
            var t = _stopwatch.Elapsed;
            MainWindow.Instance.Lbl_Timer.Text = t.TotalHours >= 1
                ? $"{(int)t.TotalHours}:{t.Minutes}:{t.Seconds:00}."
                    : t.TotalMinutes >= 1
                        ? $"{t.Minutes}:{t.Seconds:00}."
                            : $"{t.Seconds}.";
            MainWindow.Instance.Lbl_Milliseconds.Text = $"{t.Milliseconds:000}";
        }

        public static TimeSpan GetElapsedTime()
        {
            return _stopwatch.Elapsed;
        }

        public static void Start()
        {
            _stopwatch.Start();
            _timer.Start();
        }

        public static void Stop()
        {
            _stopwatch.Stop();
            _timer.Stop();
        }

        public static void Reset()
        {
            _stopwatch.Reset();
            Dispatcher.UIThread.Post(() =>
            {
                UpdateTime();
            });
        }
    }
}
