using System;
using System.Collections.Generic;

namespace Pulse.UI
{
    public sealed class UiArchiveNodeComparer : IComparer<UiArchiveNode>
    {
        public static readonly UiArchiveNodeComparer Instance = new UiArchiveNodeComparer();

        public int Compare(UiArchiveNode x, UiArchiveNode y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            
            if (y == null)
                return 1;

            if (x.Listing == null)
            {
                if (y.Listing != null)
                    return -1;
            }
            else if (y.Listing == null)
            {
                return 1;
            }

            if (x.Entry == null)
            {
                if (y.Entry != null)
                    return -1;
            }
            else if (y.Entry == null)
            {
                return 1;
            }

            return String.CompareOrdinal(x.Name, y.Name);
        }
    }
}