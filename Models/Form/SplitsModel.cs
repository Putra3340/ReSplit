using ReSplit.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ReSplit.Models.Form
{
    // Use this for form bindings
    // Anything With F_ Should be used for display purposes only, and should not be used for calculations or logic
    public class SplitsModel : INotifyPropertyChanged
    {
        // should no external reference this, only used for display purposes
        public string F_Name => Name;
        public string F_Time => NewTime != TimeSpan.Zero ? TimeSpanFormat.FormatNewTime(NewTime) : TimeSpanFormat.FormatNewTime(Time);
        public string F_DeltaTime => TimeSpanFormat.FormatDelta(DeltaTime);
        public string F_BackgroundColor => BackgroundColor;
        public string F_DeltaForegroundColor => DeltaForegroundColor;

        // This is used on the code to manipulate the display
        public string Id
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
            }
        } = string.Empty;

        public string Name
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_Name));
            }
        } = string.Empty;

        public string BackgroundColor
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_BackgroundColor));
            }
        } = "Transparent";

        public string DeltaForegroundColor
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_DeltaForegroundColor));
            }
        } = "#FFFFC000";

        public bool IsActive
        {
            get;
            set
            {
                if (field == value) return;
                field = value;

                BackgroundColor = value ? "Blue" : "Transparent";
                Update();
            }
        } = false;

        // This is Original Time, min value for skip
        public TimeSpan Time
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_Time));
            }
        } = TimeSpan.Zero;

        // Max Value for no delta, Min Value for current is new, otherwise the delta
        public TimeSpan DeltaTime
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_DeltaTime));
            }
        } = TimeSpan.MaxValue;

        // This is the new time for the split, if the split is not a new pb then this should be TimeSpan.Zero,
        // otherwise it should be the new time for the split
        public TimeSpan NewTime
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                Update();
                Update(nameof(F_Time));
            }
        } = TimeSpan.Zero;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Update([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
