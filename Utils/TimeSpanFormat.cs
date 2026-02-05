using System;
using System.Collections.Generic;
using System.Text;

namespace ReSplit.Utils
{
    public static class TimeSpanFormat
    {
        public static string FormatDelta(TimeSpan delta)
        {
            var sign = delta < TimeSpan.Zero ? "-" : "+";
            delta = delta.Duration(); // absolute value

            if (delta.TotalHours >= 1)
                return $"{sign}{(int)delta.TotalHours}:{delta.Minutes:D2}:{delta.Seconds:D2}";

            if (delta.TotalMinutes >= 1)
                return $"{sign}{(int)delta.TotalMinutes}:{delta.Seconds:D2}";

            return $"{sign}{(int)delta.TotalSeconds}";
        }
        public static string FormatNewTime(TimeSpan t)
        {
            if (t.TotalSeconds < 1)
                return "-";

            if (t.TotalHours >= 1)
                return $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}";

            return $"{t.Minutes}:{t.Seconds:00}";
        }
    }
}
