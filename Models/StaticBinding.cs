using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ReSplit.Models.Form;

namespace ReSplit.Models
{
    public static class StaticBinding
    {
        public static ObservableCollection<SplitsModel> Splits { get; set; } = new()
        {
            new SplitsModel
            {
                Name = "Example Split",
                Time = TimeSpan.FromSeconds(6767),
                DeltaTime = TimeSpan.FromSeconds(6767),
                IsActive = false,
            },
            new SplitsModel
            {
                Name = "Very Long Segments Thing",
                Time = TimeSpan.FromSeconds(6767),
                DeltaTime = TimeSpan.FromSeconds(-69),
                IsActive = false,
            },
            new SplitsModel
            {
                Name = "Lost Time",
                Time = TimeSpan.FromSeconds(69420),
                DeltaTime = TimeSpan.FromSeconds(69),
                IsActive = false,
            },
            new SplitsModel
            {
                Name = "New Time No PB",
                Time = TimeSpan.FromSeconds(69420),
                DeltaTime = TimeSpan.MinValue,
                NewTime = TimeSpan.FromSeconds(69696),
                IsActive = false,
            },
            new SplitsModel
            {
                Name = "Try Hard",
                Time = TimeSpan.FromSeconds(69467),
                DeltaTime = TimeSpan.MaxValue,
                IsActive = true,
            },

        };
        public static RunModel CurrentRun { get; set; } = new();
    }
}
