using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class WaveOutFactory : IOutputAudioDeviceFactory
    {
        private WaveOutSettingsPanel _waveOutSettingsPanel;

        public IWavePlayer CreateDevice(int latency)
        {
            IWavePlayer device;
            WaveCallbackStrategy strategy = _waveOutSettingsPanel.CallbackStrategy;
            if (strategy == WaveCallbackStrategy.Event)
            {
                WaveOutEvent waveOut = new WaveOutEvent
                {
                    DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                    DesiredLatency = latency
                };
                device = waveOut;
            }
            else
            {
                WaveCallbackInfo callbackInfo = strategy == WaveCallbackStrategy.NewWindow ? WaveCallbackInfo.NewWindow() : WaveCallbackInfo.FunctionCallback();
                WaveOut outputDevice = new WaveOut(callbackInfo)
                {
                    DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                    DesiredLatency = latency
                };
                device = outputDevice;
            }
            // TODO: configurable number of buffers

            return device;
        }

        public UserControl CreateSettingsPanel()
        {
            this._waveOutSettingsPanel = new WaveOutSettingsPanel();
            return _waveOutSettingsPanel;
        }

        public string Name
        {
            get { return "WaveOut"; }
        }

        public bool IsAvailable
        {
            get { return WaveOut.DeviceCount > 0; }
        }

        public int Priority
        {
            get { return 1; } 
        }
    }
}
