using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ArchiveEntryExtractorZtrToStrings : IArchiveEntryExtractor
    {
        private const string Extension = ".strings";

        private static readonly Lazy<ArchiveEntryExtractorZtrToStrings> LazyInstance = new Lazy<ArchiveEntryExtractorZtrToStrings>();

        public static ArchiveEntryExtractorZtrToStrings Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Extract(ArchiveAccessor archiveAccessor, ArchiveEntry entry, string targetDir, Action<long> progress)
        {
            string outputPath = Path.Combine(targetDir, Path.ChangeExtension(entry.Name, Extension));
            
            ZtrFileEntry[] entries;
            using (Stream input = archiveAccessor.ExtractBinary(entry))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(input, InteractionService.TextEncoding.Provide().Encoding);
                entries = unpacker.Unpack();
            }

            using (Stream output = File.Create(outputPath, 32 * 1024))
            {
                ZtrTextWriter writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
                writer.Write(entry.Name, entries);
            }

            progress.NullSafeInvoke(entry.UncompressedSize);
        }
    }
}