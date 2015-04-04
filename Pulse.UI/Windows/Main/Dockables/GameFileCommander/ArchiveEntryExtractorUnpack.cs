using System;
using System.IO;
using System.Threading;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ArchiveEntryExtractorUnpack : FS.IArchiveEntryExtractor
    {
        private static readonly Lazy<ArchiveEntryExtractorUnpack> LazyInstance = new Lazy<ArchiveEntryExtractorUnpack>();

        public static ArchiveEntryExtractorUnpack Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Extract(ArchiveAccessor archiveAccessor, ArchiveEntry entry, string targetDir, Action<long> progress)
        {
            byte[] buff = new byte[Math.Min(entry.UncompressedSize, 32 * 1024)];
            string outputPath = Path.Combine(targetDir, entry.Name);

            if (buff.Length == 0)
            {
                File.Create(outputPath).Dispose();
                return;
            }

            using (Stream input = archiveAccessor.OpenBinary(entry))
            using (Stream output = File.Create(outputPath, buff.Length))
            {
                if (entry.IsCompressed)
                    ZLibHelper.Uncompress(input, output, (int)entry.UncompressedSize, buff, CancellationToken.None, progress);
                else
                    input.CopyToStream(output, (int)entry.UncompressedSize, buff, progress);
            }
        }
    }
}