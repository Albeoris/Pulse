using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class XgrArchiveEntryExtractorUnpack : IXgrArchiveEntryExtractor
    {
        private static readonly Lazy<XgrArchiveEntryExtractorUnpack> LazyInstance = new Lazy<XgrArchiveEntryExtractorUnpack>();

        public static XgrArchiveEntryExtractorUnpack Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Extract(WpdEntry entry, Stream indices, Stream content, string targetDir, Action<long> progress)
        {
            byte[] buff = new byte[Math.Min(entry.Length, 32 * 1024)];
            string outputPath = Path.Combine(targetDir, entry.Name + '.' + entry.Extension);

            if (buff.Length == 0)
            {
                File.Create(outputPath).Dispose();
                return;
            }

            indices.SetReadPosition(entry.Offset);
            using (Stream output = File.Create(outputPath, buff.Length))
                indices.CopyTo(output, entry.Length, buff, progress);
        }
    }
}