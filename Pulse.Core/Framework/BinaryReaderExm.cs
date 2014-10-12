using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pulse.Core
{
    public static class BinaryReaderExm
    {
        public static Guid ReadGuid(this BinaryReader self)
        {
            Exceptions.CheckArgumentNull(self, "self");

            byte[] buff = new byte[16];
            self.BaseStream.EnsureRead(buff, 0, buff.Length);
            return new Guid(buff);
        }
    }

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