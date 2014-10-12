using System.Collections.Generic;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListing : List<ArchiveListingEntry>
    {
        public string ListingFile;
        public string BinaryFile;

        public ArchiveListing()
        {
        }

        public ArchiveListing(int entriesCount)
            : base(entriesCount)
        {
        }
    }
}