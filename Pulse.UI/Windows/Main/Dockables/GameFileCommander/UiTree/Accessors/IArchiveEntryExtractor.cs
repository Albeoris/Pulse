using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IArchiveEntryExtractor
    {
        string TargetExtension { get; }
        void Extract(ArchiveEntry entry, Stream output, Stream input, Byte[] buff);
    }
}