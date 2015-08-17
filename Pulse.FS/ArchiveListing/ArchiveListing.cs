using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class ArchiveListing : List<ArchiveEntry>, IArchiveListing
    {
        public ArchiveListing(ArchiveAccessor accessor, IArchiveListingHeader header)
            : base(header.EntriesCount)
        {
            Accessor = accessor;
            Header = header;
        }

        public readonly ArchiveAccessor Accessor;
        public readonly IArchiveListingHeader Header;

        public ArchiveListing Parent { get; set; }

        public string Name
        {
            get { return Accessor.ListingEntry.Name; }
        }
    }
}