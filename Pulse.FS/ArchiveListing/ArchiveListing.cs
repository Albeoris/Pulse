using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class ArchiveListing : List<ArchiveEntry>, IArchiveListing
    {
        public ArchiveListing(ArchiveAccessor accessor)
        {
            Accessor = accessor;
        }

        public ArchiveListing(ArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public readonly ArchiveAccessor Accessor;
        
        public ArchiveListing Parent { get; set; }

        public string Name
        {
            get { return Accessor.ListingEntry.Name; }
        }
    }
}