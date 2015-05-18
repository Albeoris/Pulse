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
        private bool isAvailable;

        public DirectSoundOutFactory()
        {
            this.isAvailable = DirectSoundOut.Devices.Count() > 0;
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

        public string Name
        {
            get { return "DirectSound"; }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        public int Priority
        {
            get { return 2; } 
        }
    }
}
