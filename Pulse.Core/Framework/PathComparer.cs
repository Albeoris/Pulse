using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pulse.Core
{
    [Serializable]
    public sealed class PathComparer : IEqualityComparer<string>
    {
        public static readonly Lazy<PathComparer> Instance = new Lazy<PathComparer>(() => new PathComparer());

        private PathComparer()
        {
        }

        public bool Equals(string x, string y)
        {
            return String.Compare(x, y, true, CultureInfo.InvariantCulture) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}