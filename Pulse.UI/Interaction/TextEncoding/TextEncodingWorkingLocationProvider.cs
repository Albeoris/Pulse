using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingWorkingLocationProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.Load();
        }

        public string Title
        {
            get { return Lang.InfoProvider.TextEncoding.WorkingLocationTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.TextEncoding.WorkingLocationDescription; }
        }
    }
}