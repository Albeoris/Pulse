using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;

namespace Pulse.UI
{
    public sealed class XgrArchiveEntryExtractorTxbhToDds : IXgrArchiveEntryExtractor
    {
        private static readonly Lazy<XgrArchiveEntryExtractorTxbhToDds> LazyInstance = new Lazy<XgrArchiveEntryExtractorTxbhToDds>();

        public static XgrArchiveEntryExtractorTxbhToDds Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Extract(WpdEntry entry, Stream indices, Stream content, string targetDir, Action<long> progress)
        {
            indices.SetReadPosition(entry.Offset);
            TextureSection textureHeader = indices.ReadContent<TextureSection>();
            GtexData data = textureHeader.Gtex;

            byte[] buff = new byte[32 * 1024];
            string outputPath = Path.Combine(targetDir, entry.Name + ".dds");

            using (Stream output = File.Create(outputPath, buff.Length))
            {
                DdsHeader header = DdsHeaderDecoder.FromGtexHeader(data.Header);
                DdsHeaderEncoder.ToFileStream(header, output);

                foreach (GtexMipMapLocation mipMap in data.MipMapData)
                {
                    content.SetReadPosition(mipMap.Offset);
                    content.CopyTo(output, mipMap.Length, buff);
                }
            }
            progress.NullSafeInvoke(entry.Length);
        }
    }
}