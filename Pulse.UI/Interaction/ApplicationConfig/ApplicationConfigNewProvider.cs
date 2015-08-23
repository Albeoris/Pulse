using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigNewProvider : IInfoProvider<ApplicationConfigInfo>
    {
        public ApplicationConfigInfo Provide()
        {
            return new ApplicationConfigInfo();
        }

        public string Title
        {
            get { return Lang.InfoProvider.ApplicationConfig.NewTitle; }
        }

        public string Description {
            get { return Lang.InfoProvider.ApplicationConfig.NewDescription; }
        }
    }
}