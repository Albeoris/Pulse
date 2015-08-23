using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationConfigurationProvider : IInfoProvider<WorkingLocationInfo>
    {
        public WorkingLocationInfo Provide()
        {
            WorkingLocationInfo value = InteractionService.Configuration.Provide().WorkingLocation;
            value.Validate();
            return value;
        }

        public string Title
        {
            get { return Lang.InfoProvider.WorkingLocation.ConfigurationTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.WorkingLocation.ConfigurationDescription; }
        }
    }
}