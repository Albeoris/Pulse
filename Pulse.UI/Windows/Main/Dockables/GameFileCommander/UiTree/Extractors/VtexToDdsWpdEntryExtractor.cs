using System;
using System.IO;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class VtexToDdsWpdEntryExtractor : IWpdEntryExtractor
    {
        public string TargetExtension
        {
            get { return "dds"; }
        }

        public void Extract(WpdEntry entry, Stream output, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff)
        {
            headers.Value.Position = entry.Offset;

            SectionHeader sectionHeader = headers.Value.ReadContent<SectionHeader>();
            VtexHeader textureHeader = headers.Value.ReadContent<VtexHeader>();
            headers.Value.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
            GtexData gtex = headers.Value.ReadContent<GtexData>();

            DdsHeader header = DdsHeaderDecoder.FromGtexHeader(gtex.Header);
            DdsHeaderEncoder.ToFileStream(header, output);

            foreach (GtexMipMapLocation mipMap in gtex.MipMapData)
            {
                content.Value.Position = mipMap.Offset;
                content.Value.CopyToStream(output, mipMap.Length, buff);
            }
        }
    }
}