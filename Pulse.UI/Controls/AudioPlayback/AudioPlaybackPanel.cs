using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;

namespace NAudioDemo.AudioPlaybackDemo
{
    public partial class AudioPlaybackPanel : UserControl
    {
        private IWavePlayer _waveOut;
        private WaveStream _waveProvider;

        public AudioPlaybackPanel(IEnumerable<IOutputAudioDeviceFactory> outputDevicePlugins)
        {
            InitializeComponent();
            LoadOutputDevicePlugins(outputDevicePlugins);
        }

        public void SetWave(WaveStream waveProvider)
        {
            this._waveProvider = waveProvider;
        }

        private void LoadOutputDevicePlugins(IEnumerable<IOutputAudioDeviceFactory> outputDevicePlugins)
        {
            comboBoxOutputDevice.DisplayMember = "Name";
            comboBoxOutputDevice.SelectedIndexChanged += comboBoxOutputDevice_SelectedIndexChanged;
            foreach (var outputDevicePlugin in outputDevicePlugins.OrderBy(p => p.Priority))
            {
                comboBoxOutputDevice.Items.Add(outputDevicePlugin);
            }
            comboBoxOutputDevice.SelectedIndex = 0;
        }

        void comboBoxOutputDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelOutputDeviceSettings.Controls.Clear();
            Control settingsPanel;
            if (SelectedOutputAudioDeviceFactory.IsAvailable)
            {
                settingsPanel = SelectedOutputAudioDeviceFactory.CreateSettingsPanel();
            }
            else
            {
                settingsPanel = new Label() { Text = "This output device is unavailable on your system", Dock=DockStyle.Fill };
            }
            panelOutputDeviceSettings.Controls.Add(settingsPanel);
        }

        private IOutputAudioDeviceFactory SelectedOutputAudioDeviceFactory
        {
            get { return (IOutputAudioDeviceFactory)comboBoxOutputDevice.SelectedItem; }
        }

        private void OnButtonPlayClick(object sender, EventArgs e)
        {
            if (!SelectedOutputAudioDeviceFactory.IsAvailable)
            {
                MessageBox.Show("The selected output driver is not available on this system");
                return;
            }

            if (_waveOut != null)
            {
                if (_waveOut.PlaybackState == PlaybackState.Playing)
                    return;
                
                if (_waveOut.PlaybackState == PlaybackState.Paused)
                {
                    _waveOut.Play();
                    groupBoxDriverModel.Enabled = false;
                    return;
                }
            }

            // we are in a stopped state
            // TODO: only re-initialise if necessary

            if (_waveProvider == null)
                return;

            try
            {
                CreateWaveOut();
            }
            catch (Exception driverCreateException)
            {
                MessageBox.Show($"{driverCreateException.Message}");
                return;
            }

            WaveStream conversionStream = WaveFormatConversionStream.CreatePcmStream(_waveProvider);


            labelTotalTime.Text = $"{(int)_waveProvider.TotalTime.TotalMinutes:00}:{_waveProvider.TotalTime.Seconds:00}";

            try
            {
                _waveOut.Init(conversionStream);
            }
            catch (Exception initException)
            {
                MessageBox.Show($"{initException.Message}", "Error Initializing Output");
                return;
            }

            groupBoxDriverModel.Enabled = false;
            _waveOut.Play();
        }

        private void CreateWaveOut()
        {
            CloseWaveOut();
            int latency = 300;
            _waveOut = SelectedOutputAudioDeviceFactory.CreateDevice(latency);
            _waveOut.PlaybackStopped += OnPlaybackStopped;
        }

        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            groupBoxDriverModel.Enabled = true;
            if (e.Exception != null)
            {
                MessageBox.Show(e.Exception.Message, "Playback Device Error");
            }
            if (_waveProvider != null)
            {
                _waveProvider.Position = 0;
            }
        }

        private void CloseWaveOut()
        {
            _waveOut?.Stop();

            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }
        }

        private void OnButtonPauseClick(object sender, EventArgs e)
        {
            if (_waveOut?.PlaybackState == PlaybackState.Playing)
                _waveOut.Pause();
        }

        private void OnButtonStopClick(object sender, EventArgs e)
        {
            _waveOut?.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_waveOut != null && _waveProvider != null)
            {
                TimeSpan currentTime = (_waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : _waveProvider.CurrentTime;
                trackBarPosition.Value = Math.Min(trackBarPosition.Maximum, (int)(100 * currentTime.TotalSeconds / _waveProvider.TotalTime.TotalSeconds));
                labelCurrentTime.Text = $"{(int)currentTime.TotalMinutes:00}:{currentTime.Seconds:00}";
            }
            else
            {
                trackBarPosition.Value = 0;
            }
        }

        private void trackBarPosition_Scroll(object sender, EventArgs e)
        {
            if (_waveOut != null)
            {
                _waveProvider.CurrentTime = TimeSpan.FromSeconds(_waveProvider.TotalTime.TotalSeconds * trackBarPosition.Value / 100.0);
            }
        }
    }
}

