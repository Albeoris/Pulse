using System.Linq;
using NAudio.Wave;

namespace Pulse.UI
{
    public sealed class AudioSettingsInfo
    {
        public IWavePlayer CreateWavePlayer()
        {
            return new DirectSoundOut(DirectSoundOut.Devices.First().Guid);
        }
    }
}