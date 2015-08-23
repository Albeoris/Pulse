using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IWpdEntryExtractor
    {
        string TargetExtension { get; }
        void Extract(WpdEntry entry, Stream output, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff);
    }
}