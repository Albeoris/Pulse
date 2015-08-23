using System;
using System.Collections.Generic;

namespace Pulse.UI
{
    public sealed class UiNodeComparer : IComparer<UiNode>
    {
        public static readonly UiNodeComparer Instance = new UiNodeComparer();

        public int Compare(UiNode x, UiNode y)
        {
            if (x == null)
                return y == null ? 0 : -1;

            if (y == null)
                return 1;

            if (x.Type < y.Type)
                return -1;

            if (x.Type > y.Type)
                return 1;

            return String.CompareOrdinal(x.Name, y.Name);
        }
    }
}