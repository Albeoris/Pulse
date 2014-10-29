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
            get { return "Настройки"; }
        }

        public string Description
        {
            get { return "Использует информацию, сохранённую в настроечном файле."; }
        }
    }
}