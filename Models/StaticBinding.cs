using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ReSplit.Models.Form;

namespace ReSplit.Models
{
    public static class StaticBinding
    {
        public static ObservableCollection<SplitsModel> Splits { get; set; } = new();
        public static RunModel CurrentRun { get; set; } = new();
    }
}
