using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    public interface IOutputAudioDeviceFactory
    {
        IWavePlayer CreateDevice(int latency);
        UserControl CreateSettingsPanel();
        string Name { get; }
        bool IsAvailable { get; }
        int Priority { get; }
    }
}
