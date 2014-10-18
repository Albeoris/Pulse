using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Pulse.Core;

namespace Pulse.UI
{
    public static class BitmapConverter
    {
        public static BitmapSource ToBitmapSource(Bitmap source)
        {
            BitmapSource bitSrc;

            IntPtr bitmap = source.GetHbitmap();

            try
            {
                bitSrc = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                NativeMethods.DeleteObject(bitmap);
            }

            return bitSrc;
        }
    }
}