using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Pulse.UI
{
    public static class Icons
    {
        public static BitmapSource DiskIcon
        {
            get { return LazyDiskIcon.Value; }
        }

        public static BitmapSource FolderIcon
        {
            get { return LazyFolderIcon.Value; }
        }

        public static BitmapSource TxtFileIcon
        {
            get { return LazyTxtFileIcon.Value; }
        }

        private static readonly Lazy<BitmapSource> LazyDiskIcon = new Lazy<BitmapSource>(CreateDiskIcon);
        private static readonly Lazy<BitmapSource> LazyFolderIcon = new Lazy<BitmapSource>(CreateDirectoryIcon);
        private static readonly Lazy<BitmapSource> LazyTxtFileIcon = new Lazy<BitmapSource>(() => CreateFileIcon(".txt"));

        private static BitmapSource CreateDiskIcon()
        {
            string disk = Path.GetPathRoot(Path.GetTempPath());
            return ShellHelper.ExtractAssociatedIcon(disk, false);
        }

        private static BitmapSource CreateDirectoryIcon()
        {
            string folder = Path.GetTempPath();
            return ShellHelper.ExtractAssociatedIcon(folder, false);
        }

        private static BitmapSource CreateFileIcon(string extension)
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
            File.Create(path).Close();
            try
            {
                return ShellHelper.ExtractAssociatedIcon(path, false);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}