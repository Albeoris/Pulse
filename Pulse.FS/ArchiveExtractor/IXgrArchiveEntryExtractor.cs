using System;
using System.IO;

namespace Pulse.FS
{
    public interface IXgrArchiveEntryExtractor
    {
        void Extract(WpdEntry entry, Stream indices, Stream content, string targetDir, Action<long> progress);
    }
}