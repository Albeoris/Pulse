using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeViewItem : TreeViewItem
    {
        public IEnumerable<ArchiveListingEntry> EnumerateFiles()
        {
            if (Items.Count == 0)
            {
                ArchiveListingEntry entry = Tag as ArchiveListingEntry;
                if (entry != null)
                    yield return entry;
            }
            else
            {
                foreach (ArchiveListingEntry entry in Items.Cast<UiArchiveTreeViewItem>().SelectMany(child => child.EnumerateFiles()))
                    yield return entry;
            }
        }

        public ArchiveListing GetParentListing()
        {
            UiArchiveTreeViewItem item = this;
            while (item.Parent as UiArchiveTreeViewItem != null)
                item = (UiArchiveTreeViewItem)item.Parent;
            return (ArchiveListing)item.Tag;
        }

        public ArchiveListing CreateChildListing()
        {
            ArchiveListing parentListing = GetParentListing();
            ArchiveListing listing = new ArchiveListing {ListingFile = parentListing.ListingFile, BinaryFile = parentListing.BinaryFile};
            foreach (ArchiveListingEntry entry in EnumerateFiles())
                listing.Add(entry);
            listing.TrimExcess();
            return listing;
        }
    }
}