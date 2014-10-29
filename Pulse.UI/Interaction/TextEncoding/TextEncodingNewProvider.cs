namespace Pulse.UI
{
    public sealed class TextEncodingNewProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.CreateDefault();
        }

        public string Title
        {
            get { return "Новая"; }
        }

        public string Description
        {
            get { return "Создает стандартный набор кодировок."; }
        }
    }
}