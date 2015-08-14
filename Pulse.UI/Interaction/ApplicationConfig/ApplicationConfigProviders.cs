using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigProviders : InfoProviderGroup<ApplicationConfigInfo>
    {
        public ApplicationConfigProviders()
            : base(Lang.InfoProvider.ApplicationConfig.Title, Lang.InfoProvider.ApplicationConfig.Description)
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

        public void LocalizatorEnvironmentProvided(LocalizatorEnvironmentInfo obj)
        {
            ApplicationConfigInfo config = Provide();
            config.LocalizatorEnvironment = obj;
            config.Save();
        }
    }
}