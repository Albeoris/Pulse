using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherExitButton : UiProgressButton
    {
        private const string ExitLabel = "Выход";
        private const string ExitingLabel = "Выход...";

        public UiPatcherExitButton()
        {
            Label = ExitLabel;
        }

        protected override async Task DoAction()
        {
            Label = ExitingLabel;
            try
            {
                ((Window)this.GetRootElement()).Close();
            }
            finally
            {
                Label = ExitLabel;
            }
        }
    }
}