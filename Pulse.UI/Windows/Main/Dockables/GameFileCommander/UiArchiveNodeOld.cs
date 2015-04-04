//using System.Collections.Generic;
//using System.Linq;
//using Pulse.Core;
//using Pulse.FS;

//namespace Pulse.UI
//{
//    public class UiArchiveNodeOld
//    {
//        public string Name { get; set; }
//        public IArchiveListing Listing { get; set; }
//        public IArchiveEntry Entry { get; set; }
//        public UiArchiveNodeOld Parent { get; set; }
//        public virtual UiArchiveNodeOld[] Childs { get; set; }

//        public IEnumerable<IArchiveListing> CreateChildListing(ArchiveListing archiveListing, Wildcard wildcard)
//        {
//            ArchiveListing listing = new ArchiveListing(archiveListing.Accessor) {FullListing = archiveListing};
//            foreach (UiArchiveNodeOld child in EnumerateChilds())
//            {
//                ArchiveEntry entry = child.Entry as ArchiveEntry;
//                if (entry != null)
//                {
//                    if (wildcard == null || wildcard.IsMatch(entry.Name))
//                        listing.Add(entry);
//                }

//                XgrArchiveListing xgr = child.Listing as XgrArchiveListing;
//                if (xgr != null)
//                    yield return CreateXgrChildListing(child, wildcard, archiveListing);
//            }
//            listing.TrimExcess();
//            yield return listing;
//        }

//        public XgrArchiveListing CreateXgrChildListing(UiArchiveNodeOld archiveNodeOld, Wildcard wildcard, ArchiveListing archiveListing)
//        {
//            XgrArchiveListing fullListing = (XgrArchiveListing)archiveNodeOld.Listing;
//            XgrArchiveListing listing = new XgrArchiveListing(fullListing.Accessor) {FullListing = fullListing, ParentArchiveListing = archiveListing};

//            foreach (UiArchiveNodeOld childNode in archiveNodeOld.Childs)
//            {
//                if (childNode.IsChecked != true)
//                    continue;

//                if (wildcard == null)
//                {
//                    listing.Add((WpdEntry)childNode.Entry);
//                    continue;
//                }

//                WpdEntry wpdEntry = (WpdEntry)childNode.Entry;
//                string fullName = wpdEntry.Name + '.' + wpdEntry.Extension;
//                if (wildcard.IsMatch(fullName))
//                    listing.Add((WpdEntry)childNode.Entry);
//            }
//            listing.TrimExcess();
//            return listing;
//        }

//        private IEnumerable<UiArchiveNodeOld> EnumerateChilds()
//        {
//            if (IsChecked == false)
//                yield break;

//            if (Entry != null || Listing is XgrArchiveListing)
//            {
//                yield return this;
//                yield break;
//            }

//            foreach (UiArchiveNodeOld entry in Childs.Where(c => c.IsChecked != false).SelectMany(child => child.EnumerateChilds()))
//                yield return entry;
//        }
//    }
//}