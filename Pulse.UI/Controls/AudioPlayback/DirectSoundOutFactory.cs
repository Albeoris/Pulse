using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class DirectSoundOutFactory : IOutputAudioDeviceFactory
    {
        private DirectSoundOutSettingsPanel settingsPanel;

        public DirectSoundOutFactory()
        {
            this.IsAvailable = DirectSoundOut.Devices.Any();
        }

        public IWavePlayer CreateDevice(int latency)
        {
            return new DirectSoundOut(settingsPanel.SelectedDevice, latency);
        }

        public UserControl CreateSettingsPanel()
        {
            this.settingsPanel = new DirectSoundOutSettingsPanel();
            return this.settingsPanel;
        }

        public string Name => "DirectSound";
        public bool IsAvailable { get; }
        public int Priority => 2;
    }
}
