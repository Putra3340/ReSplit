using System;
using System.Collections.Generic;
using System.Text;

namespace ReSplit.Utils
{
    public static class TimeSpanFormat
    {
        public static string FormatDelta(TimeSpan delta)
        {
            if (delta == TimeSpan.MinValue) return "-";
            if (delta == TimeSpan.MaxValue) return "";
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
            if(t == TimeSpan.MinValue) return "-";
            if (t.TotalSeconds < 1)
                return "00:00";

            if (t.TotalHours >= 1)
                return $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}";

            return $"{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }
}
