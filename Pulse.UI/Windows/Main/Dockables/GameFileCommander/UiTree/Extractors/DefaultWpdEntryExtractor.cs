using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DefaultWpdEntryExtractor : IWpdEntryExtractor
    {
        public string TargetExtension
        {
            get { return String.Empty; }
        }

        public void Extract(WpdEntry entry, Stream output, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff)
        {
            headers.Value.Position = entry.Offset;
            headers.Value.CopyToStream(output, entry.Length, buff);
        }
    }
}