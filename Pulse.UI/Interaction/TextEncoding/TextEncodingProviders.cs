namespace Pulse.UI
{
    public sealed class TextEncodingProviders : InfoProviderGroup<TextEncodingInfo>
    {
        public TextEncodingProviders()
            :base("Кодировки", "Кодировки для распаковки и упаковки игровых текстов.")
        {
            Capacity = 2;
            Add(new TextEncodingConfigurationProvider());
            Add(new TextEncodingNewProvider());
            //Add(new TextEncodingUserProvider());
        }
    }
}