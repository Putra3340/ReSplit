using System;
using System.Collections.Generic;
using System.Text;

namespace ReSplit.Models.Form
{
    public enum TimerState
    {
        NotStarted,
        Running,
        Paused,
        LosingTime,
        GainingTime,
        Ended
    }
}
