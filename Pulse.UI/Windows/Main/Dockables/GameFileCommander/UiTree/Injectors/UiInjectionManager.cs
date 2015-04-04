using System.Collections.Generic;
using System.Linq;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiInjectionManager
    {
        private readonly HashSet<ArchiveListing> _set = new HashSet<ArchiveListing>();

        public void Enqueue(ArchiveListing parent)
        {
            _set.Add(parent);
        }

        public void WriteListings()
        {
            HashSet<ArchiveListing> set = new HashSet<ArchiveListing>();
            foreach (ArchiveListing listing in _set)
            {
                ArchiveListing item = listing;
                while (item != null && set.Add(item))
                    item = item.Parent;
            }

            foreach (ArchiveListing listing in set.OrderByDescending(l => l.Accessor.Level))
                ArchiveListingWriter.Write(listing);
        }
    }
}