using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Pulse.Core;

namespace Pulse.UI
{
    public static class ShellHelper
    {
        public static BitmapSource ExtractAssociatedIcon(string path, bool large)
        {
            ShellFileSystemInfo info = new ShellFileSystemInfo();
            NativeMethods.SHGetFileInfo(path, 0, ref info, Marshal.SizeOf(info), SHGetFileInfoFlags.Icon | (large ? SHGetFileInfoFlags.LargeIcon : SHGetFileInfoFlags.SmallIcon));
            using (Icon icon = Icon.FromHandle(info.IconHandle))
            using (Bitmap bitmap = icon.ToBitmap())
                return BitmapConverter.ToBitmapSource(bitmap);
        }
    }
}