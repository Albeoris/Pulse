using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DefaultArchiveEntryExtractor : IArchiveEntryExtractor
    {
        public string TargetExtension
        {
            get { return String.Empty; }
        }

        public void Extract(ArchiveEntry entry, StreamSequence output, Stream input, Byte[] buff)
        {
            int size = (int)entry.UncompressedSize;
            if (size == 0)
                return;

            input.CopyToStream(output, size, buff);
        }
    }
}