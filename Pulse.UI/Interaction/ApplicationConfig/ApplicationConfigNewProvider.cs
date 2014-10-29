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
            get { return "Новый"; }
        }

        public string Description {
            get { return "Инициализирует все настройки значениями по умолчанию."; }
        }
    }
}