using System;
using System.Collections.Generic;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ArchiveListingInjectComparer : IComparer<IArchiveListing>
    {
        public static readonly ArchiveListingInjectComparer Instance = new ArchiveListingInjectComparer();

        public int Compare(IArchiveListing x, IArchiveListing y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            
            if (y == null)
                return 1;

            ArchiveListing xList = x as ArchiveListing;
            ArchiveListing yList = y as ArchiveListing;

            WpdArchiveListing xWpd = x as WpdArchiveListing;
            WpdArchiveListing yWpd = y as WpdArchiveListing;

            if (xList != null && yList != null)
                return xList.Accessor.Level.CompareTo(yList.Accessor.Level) * -1;

            if (xWpd != null && yWpd != null)
                return String.Compare(x.Name, y.Name, StringComparison.Ordinal);

            return xWpd != null ? 1 : -1;
        }
    }
}