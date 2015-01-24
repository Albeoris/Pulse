using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class XgrArchiveListing : List<WpdEntry>, IArchiveListing
    {
        public readonly XgrArchiveAccessor Accessor;

        public XgrArchiveListing(XgrArchiveAccessor accessor)
        {
            Accessor = accessor;
        }

        public XgrArchiveListing(XgrArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public string Name
        {
            get { return Accessor.Name; }
        }

        public ArchiveListing ParentArchiveListing { get; set; }
        public XgrArchiveListing FullListing { get; set; }
    }
}