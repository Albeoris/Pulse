using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class ArchiveListing : List<ArchiveEntry>, IArchiveListing
    {
        public readonly ArchiveAccessor Accessor;

        public string Name
        {
            get { return Accessor.ListingEntry.Name; }
        }

        public ArchiveListing FullListing { get; set; }

        public ArchiveListing(ArchiveAccessor accessor)
        {
            Accessor = accessor;
        }

        public ArchiveListing(ArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }
    }
}