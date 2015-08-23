using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IArchiveEntryExtractor
    {
        string TargetExtension { get; }
        void Extract(ArchiveEntry entry, StreamSequence output, Stream input, Byte[] buff);
    }
}