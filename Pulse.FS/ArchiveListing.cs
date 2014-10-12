using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class ArchiveListing : List<ArchiveListingEntry>
    {
        public ArchiveListing()
        {
        }

        public ArchiveListing(int entriesCount)
            : base(entriesCount)
        {
        }
    }
}