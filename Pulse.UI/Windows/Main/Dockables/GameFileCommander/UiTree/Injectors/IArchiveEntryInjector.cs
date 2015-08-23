using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IArchiveEntryInjector
    {
        string SourceExtension { get; }
        bool TryInject(IUiInjectionSource source, string sourceFullPath, ArchiveEntryInjectionData data, ArchiveEntry entry);
    }
}