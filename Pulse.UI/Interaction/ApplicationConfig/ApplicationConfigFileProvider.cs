using System;
using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigFileProvider : IInfoProvider<ApplicationConfigInfo>
    {
        public ApplicationConfigInfo Provide()
        {
            ApplicationConfigInfo result = new ApplicationConfigInfo();
            result.Load();
            return result;
        }

        public string Title
        {
            get { return Lang.InfoProvider.ApplicationConfig.FileTitle; }
        }

        public string Description
        {
            get { return String.Format(Lang.InfoProvider.ApplicationConfig.FileDescription, ApplicationConfigInfo.ConfigurationFilePath); }
        }
    }
}