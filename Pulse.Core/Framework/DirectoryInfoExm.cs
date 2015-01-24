using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pulse.Core
{
    public static class DirectoryInfoExm
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            Exceptions.CheckArgumentNullOrEmprty(extensions, "extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension, PathComparer.Instance.Value));
        }
    }
}