using System.IO;
using System.Linq;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class GtexData : IStreamingContent
    {
        public GtexHeader Header;
        public GtexMipMapLocation[] MipMapData;

        public void ReadFromStream(Stream input)
        {
            Header = input.ReadContent<GtexHeader>();
            MipMapData = input.ReadContent<GtexMipMapLocation>(Header.LayerCount);
        }

        public void WriteToStream(Stream output)
        {
            output.WriteContent(Header);
            output.WriteContent(MipMapData);
        }
    }
}