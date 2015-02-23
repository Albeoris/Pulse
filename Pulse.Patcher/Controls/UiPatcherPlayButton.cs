using System.Diagnostics;
using System.Threading.Tasks;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherPlayButton : UiProgressButton
    {
        private const string PlayLabel = "Играть";
        private const string PlayingLabel = "Запуск...";

        public UiPatcherPlayButton()
        {
            Label = PlayLabel;
        }

        protected override async Task DoAction()
        {
            Label = PlayingLabel;
            try
            {
                Maximum = 2;

                InteractionService.SetGamePart(FFXIIIGamePart.Part1);
                GameLocationInfo gameLocation = PatcherService.GetGameLocation(FFXIIIGamePart.Part1);
                gameLocation.Validate();
                Position = 1;

                if (CancelEvent.WaitOne(0))
                    return;

                await Task.Factory.StartNew(() => Process.Start(gameLocation.ExecutablePath));
                Position = 2;
            }
            finally
            {
                Label = PlayLabel;
            }
        }
    }
}