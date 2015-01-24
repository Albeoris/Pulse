using System;

namespace Pulse.FS
{
    public interface IArchiveEntryInjector
    {
        int CalcSize();
        void Inject(ArchiveAccessor archiveAccessor, bool? wantCompress, Action<long> progress);
    }
}