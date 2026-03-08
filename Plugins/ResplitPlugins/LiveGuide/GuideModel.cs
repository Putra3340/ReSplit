using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace LiveGuide
{
    public class GuideModel : INotifyPropertyChanged
    {
        public string Id { get; set { field = value; Update(); } }
        public string Name { get; set { field = value; Update(); } }
        public string Description { get; set { field = value; Update(); } }
        public string Text { get; set { field = value; Update(); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void Update([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
