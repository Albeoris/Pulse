using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationProviders : InfoProviderGroup<GameLocationInfo>
    {
        public GameLocationProviders()
            : base(Lang.InfoProvider.GameLocation.Title, Lang.InfoProvider.GameLocation.Description)
        {
            Capacity = 3;
            Add(new GameLocationConfigurationProvider());
            Add(new GameLocationSteamRegistryProvider());
            Add(new GameLocationUserProvider());
        }
    }
}