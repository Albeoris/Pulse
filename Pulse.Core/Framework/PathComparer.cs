using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
            if (x == null)
                return y == null;
            if (y == null)
                return false;
            
            string[] xParts = x.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string[] yParts = y.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (xParts.Length != yParts.Length)
                return false;

            for (int i = 0; i < xParts.Length; i++)
            {
                if (String.Compare(xParts[i], yParts[i], true, CultureInfo.InvariantCulture) != 0)
                    return false;
            }
            
            return true;
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}