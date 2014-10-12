using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public static class PathEx
    {
        public static string ChangeName(string filePath, string newName)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
                return newName;

            return Path.Combine(directory, newName);
        }
    }
}
