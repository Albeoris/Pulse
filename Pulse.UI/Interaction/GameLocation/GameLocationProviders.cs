namespace Pulse.UI
{
    public sealed class GameLocationProviders : InfoProviderGroup<GameLocationInfo>
    {
        public GameLocationProviders()
            :base("Каталог игры", "Местоположении игровых файлов.")
        {
            Capacity = 3;
            Add(new GameLocationConfigurationProvider());
            Add(new GameLocationSteamRegistryProvider());
            Add(new GameLocationUserProvider());
        }
    }
}