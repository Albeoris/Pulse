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
        private const string ExitLabel = "�����";
        private const string ExitingLabel = "�����...";

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