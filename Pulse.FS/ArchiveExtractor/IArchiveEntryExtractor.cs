using System;
using System.IO;

namespace Pulse.FS
{
    public interface IArchiveEntryExtractor
    {
        void Extract(ArchiveAccessor archiveAccessor, ArchiveEntry entry, string targetDir, Action<long> progress);
    }
}