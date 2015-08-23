using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherTrackerButton : UiRoundButton
    {
        private const string TrackerLabel = "Ошибка?";

        public UiPatcherTrackerButton()
        {
            Label = TrackerLabel;
        }

        protected override async Task DoAction()
        {
            await Task.Run(() => Process.Start("http://ff13.ffrtt.ru/tracker.php"));
        }
    }
}