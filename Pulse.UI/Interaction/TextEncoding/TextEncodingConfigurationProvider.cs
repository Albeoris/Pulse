using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingConfigurationProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.Load();
        }

        public string Title
        {
            get { return "Из рабочего каталога"; }
        }

        public string Description
        {
            get { return "Использует информацию, сохранённую в настроечном файле."; }
        }
    }
}