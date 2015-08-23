using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Pulse.DirectX;

namespace Pulse.UI
{
    public sealed class DdsToTxbhWpdEntryInjector : IWpdEntryInjector
    {
        public string SourceExtension
        {
            get { return "dds"; }
        }

        public void Inject(WpdEntry entry, Stream input, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff)
        {
            int sourceSize = (int)input.Length;
            headers.Value.Position = entry.Offset;

            TextureSection textureHeader = headers.Value.ReadContent<TextureSection>();
            GtexData data = textureHeader.Gtex;
            if (data.MipMapData.Length != 1)
                throw new NotImplementedException();

            DdsHeader ddsHeader = DdsHeaderDecoder.FromFileStream(input);
            DdsHeaderEncoder.ToGtexHeader(ddsHeader, data.Header);

            GtexMipMapLocation mipMapLocation = data.MipMapData[0];
            int dataSize = sourceSize - 128;
            if (dataSize <= mipMapLocation.Length)
            {
                content.Value.Seek(mipMapLocation.Offset, SeekOrigin.Begin);
            }
            else
            {
                content.Value.Seek(0, SeekOrigin.End);
                mipMapLocation.Offset = (int)content.Value.Position;
            }

            input.CopyToStream(content.Value, dataSize, buff);
            mipMapLocation.Length = dataSize;

            using (MemoryStream ms = new MemoryStream(96))
            {
                textureHeader.WriteToStream(ms);
                ms.SetPosition(0);

                DefaultWpdEntryInjector defaultInjector = new DefaultWpdEntryInjector();
                defaultInjector.Inject(entry, ms, headers, content, buff);
            }
        }
    }
}