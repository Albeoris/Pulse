namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigProviders : InfoProviderGroup<ApplicationConfigInfo>
    {
        public ApplicationConfigProviders()
            : base("Настройки", "Различные настройки приложения.")
        {
            Capacity = 2;
            Add(new ApplicationConfigFileProvider());
            Add(new ApplicationConfigNewProvider());
        }

        public void GameLocationProvided(GameLocationInfo obj)
        {
            ApplicationConfigInfo config = Provide();
            config.GameLocation = obj;
            config.Save();
        }

        public void WorkingLocationProvided(WorkingLocationInfo obj)
        {
            ApplicationConfigInfo config = Provide();
            config.WorkingLocation = obj;
            config.Save();
        }
    }
}