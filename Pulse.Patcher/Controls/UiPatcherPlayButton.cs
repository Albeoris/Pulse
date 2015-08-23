using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Pulse.Core;
using Pulse.Patcher.Controls;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherPlayButton : UiProgressButton
    {
        private const string PlayLabel = "Играть";
        private const string PlayingLabel = "Запуск...";

        public GameSettingsControl GameSettings { get; set; }
        public BackgroundMusicPlayer MusicPlayer { get; set; }

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

                GameLocationInfo gameLocation = PatcherService.GetGameLocation(FFXIIIGamePart.Part1);
                gameLocation.Validate();
                Position = 1;

                if (CancelEvent.WaitOne(0))
                    return;

                if (MusicPlayer != null && MusicPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    MusicPlayer.Pause();

                String args = GameSettings.GetGameProcessArguments();

                await Task.Factory.StartNew(() => Process.Start(gameLocation.ExecutablePath, args));
                Position = 2;

                if (InteractionService.LocalizatorEnvironment.Provide().ExitAfterRunGame)
                    Application.Current.MainWindow.Close();
            }
            finally
            {
                Label = PlayLabel;
            }
        }
    }
}