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
            get { return "Из файла"; }
        }

        public string Description
        {
            get { return @"Считывает настройки приложения из файла " + ApplicationConfigInfo.ConfigurationFilePath; }
        }
    }
}