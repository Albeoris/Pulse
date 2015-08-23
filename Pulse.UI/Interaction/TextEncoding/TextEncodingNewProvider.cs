using Pulse.Core;

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
            get { return Lang.InfoProvider.TextEncoding.NewTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.TextEncoding.NewDescription; }
        }
    }
}