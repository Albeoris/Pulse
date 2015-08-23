using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ZtrToStringsArchiveEntryExtractor : IArchiveEntryExtractor
    {
        public string TargetExtension
        {
            get { return ".strings"; }
        }

        public void Extract(ArchiveEntry entry, StreamSequence output, Stream input, Byte[] buff)
        {
            int size = (int)entry.UncompressedSize;
            if (size == 0)
                return;

            ZtrFileUnpacker unpacker = new ZtrFileUnpacker(input, InteractionService.TextEncoding.Provide().Encoding);
            ZtrFileEntry[] entries = unpacker.Unpack();

            ZtrTextWriter writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
            writer.Write(entry.Name, entries);
        }
    }
}