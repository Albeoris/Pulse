using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IWpdEntryInjector
    {
        string SourceExtension { get; }
        void Inject(WpdEntry entry, Stream input, Lazy<Stream> headers, Lazy<Stream> content, Byte[] buff);
    }
}