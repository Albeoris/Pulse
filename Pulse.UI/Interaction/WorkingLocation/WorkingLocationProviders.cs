namespace Pulse.UI
{
    public sealed class WorkingLocationProviders : InfoProviderGroup<WorkingLocationInfo>
    {
        public WorkingLocationProviders()
            :base("Рабочий каталог", "Директория для хранения результатов работы приложения.")
        {
            Capacity = 2;
            Add(new WorkingLocationConfigurationProvider());
            Add(new WorkingLocationUserProvider());
        }

        public void TextEncodingProvided(TextEncodingInfo obj)
        {
            obj.Save();
        }
    }
}