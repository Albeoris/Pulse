using Pulse.Core;

namespace Pulse.UI
{
    public sealed class AudioSettingsProviders : InfoProviderGroup<AudioSettingsInfo>
    {
        public AudioSettingsProviders()
            :base(Lang.InfoProvider.AudioSettings.Title, Lang.InfoProvider.AudioSettings.Description)
        {
            Capacity = 1;
            Add(new AudioSettingsNewProvider());
        }
    }
}