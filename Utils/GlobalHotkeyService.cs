using Avalonia.Controls;
using ReSplit.Models;
using ReSplit.Models.Form;
using SharpHook;
using SharpHook.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReSplit.Utils
{
    public class GlobalHotkeyService
    {
        private readonly SimpleGlobalHook _hook = new();

        public GlobalHotkeyService()
        {
            _hook.KeyPressed += KeyPressed;

            Task.Run(() => _hook.Run());
        }

        private void KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcNumPad0)
                CentralControls.StartNewAttempt();
            if (e.Data.KeyCode == KeyCode.VcNumPad9)
                CentralControls.ResetRun();
            if(e.Data.KeyCode == KeyCode.VcNumPad4)
                CentralControls.UndoSplit();
            if(e.Data.KeyCode == KeyCode.VcNumPad6)
                CentralControls.SkipSplit();
            if (e.Data.KeyCode == KeyCode.VcNumPad7)
                CentralControls.Pause();
        }

        public void Stop()
        {
            _hook.Dispose();
        }
    }
}
