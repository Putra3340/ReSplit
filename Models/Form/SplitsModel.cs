using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ReSplit.Models.Form
{
    // Use this for form bindings
    public class SplitsModel : INotifyPropertyChanged
    {
        public string Id { get; set { field = value; OnPropertyChanged(); } } = Guid.NewGuid().ToString();
        public string F_Name { get; set { field = value; OnPropertyChanged(); } } = "";
        public string F_Time { get; set { field = value; OnPropertyChanged(); } } = "";
        public string F_DeltaTime { get; set { field = value; OnPropertyChanged(); } } = "";
        public string BackgroundColor { get; set { field = value; OnPropertyChanged(); } } = "#00000000";
        public string DeltaForegroundColor { get; set { field = value; OnPropertyChanged(); } } = "#FFFFC000";
        public bool IsActive { get; set { field = value; if (value) BackgroundColor = "#A00000E0"; if (!value) BackgroundColor = "#00000000"; OnPropertyChanged(); } } = false;
        public TimeSpan Time { get; set { field = value; OnPropertyChanged(); } } = TimeSpan.Zero;
        public TimeSpan DeltaTime { get; set { field = value; OnPropertyChanged(); } } = TimeSpan.Zero;
        public TimeSpan NewTime { get; set { field = value; OnPropertyChanged(); } } = TimeSpan.Zero;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
