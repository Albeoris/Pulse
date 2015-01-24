using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationProviders : InfoProviderGroup<WorkingLocationInfo>
    {
        public WorkingLocationProviders()
            :base(Lang.InfoProvider.WorkingLocation.Title, Lang.InfoProvider.WorkingLocation.Description)
        {
            Capacity = 2;
            Add(new WorkingLocationConfigurationProvider());
            Add(new WorkingLocationUserProvider());
        }
    }
}