namespace Pulse.UI
{
    public sealed class LocalizatorEnvironmentProviders : InfoProviderGroup<LocalizatorEnvironmentInfo>
    {
        public LocalizatorEnvironmentProviders()
            : base("LocalizatorEnvironment", string.Empty)
        {
            Capacity = 2;
            Add(new LocalizatorEnvironmentConfigurationProvider());
            Add(new LocalizatorEnvironmentNewProvider());
        }
    }
}