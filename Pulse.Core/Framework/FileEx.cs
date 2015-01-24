using System;
using System.IO;
using System.Linq;

namespace Pulse.Core
{
    public static class FileEx
    {
        public static long GetSize(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                return fileStream.Length;
        }

        public static long GetSize(FileSystemInfo fsi)
        {
            Exceptions.CheckArgumentNull(fsi, "fsi");

            try
            {
                FileInfo fileInfo = fsi as FileInfo;
                if (fileInfo != null)
                    return fileInfo.Length;

                DirectoryInfo directoryInfo = fsi as DirectoryInfo;
                if (directoryInfo != null)
                    return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);

                Log.Warning("[FileEx]Неизвестный наследник FileSystemInfo: {0}", fsi.GetType());
                return 0;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[FileEx]Непредвиденная ошибка.");
                return 0;
            }
        }
    }
}