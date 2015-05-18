using Pulse.Core;

namespace Pulse.UI
{
    public sealed class AudioSettingsNewProvider : IInfoProvider<AudioSettingsInfo>
    {
        public AudioSettingsInfo Provide()
        {
            return new AudioSettingsInfo();
        }

        public string Title
        {
            get { return Lang.InfoProvider.AudioSettings.NewTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.AudioSettings.NewDescription; }
        }
    }
}