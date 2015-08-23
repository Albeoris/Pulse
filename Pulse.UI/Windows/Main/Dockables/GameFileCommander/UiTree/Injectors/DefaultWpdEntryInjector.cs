using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DefaultWpdEntryInjector : IWpdEntryInjector
    {
        public string SourceExtension
        {
            get { return String.Empty; }
        }

        public void Inject(WpdEntry entry, Stream input, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff)
        {
            int sourceSize = (int)input.Length;
            if (sourceSize <= entry.Length)
            {
                headers.Value.Seek(entry.Offset, SeekOrigin.Begin);
            }
            else
            {
                headers.Value.Seek(0, SeekOrigin.End);
                entry.Offset = (int)headers.Value.Position;
            }

            input.CopyToStream(headers.Value, sourceSize, buff);
            entry.Length = sourceSize;
        }
    }
}