using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pulse.Core;
using Color = System.Windows.Media.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Pulse.OpenGL
{
    public sealed class BitmapPalettesGLTextureReader : GLTextureReader
    {
        private readonly BitmapPalette[] _palettes;

        public BitmapPalettesGLTextureReader(params BitmapPalette[] palettes)
        {
            _palettes = palettes;
        }

        public override async Task<GLTexture> ReadTextureAsync(CancellationToken cancelationToken)
        {
            if (_palettes.IsNullOrEmpty() || cancelationToken.IsCancellationRequested)
                return RaiseTextureReaded(null);

            using (Bitmap bitmap = new Bitmap(256, _palettes.Length, PixelFormat.Format32bppArgb))
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                if (cancelationToken.IsCancellationRequested)
                    return RaiseTextureReaded(null);

                int size = bitmapData.Stride * bitmapData.Height;
                using (UnmanagedMemoryStream output = bitmapData.Scan0.OpenStream(size, FileAccess.Write))
                {
                    foreach (BitmapPalette palette in _palettes)
                    {
                        if (cancelationToken.IsCancellationRequested)
                            return RaiseTextureReaded(null);

                        Convert(palette, output);
                    }
                }

                bitmap.UnlockBits(bitmapData);

                BitmapGLTextureReader bitmapReader = new BitmapGLTextureReader(bitmap, bitmap.Width, bitmap.Height, bitmap.PixelFormat);
                return RaiseTextureReaded(await bitmapReader.ReadTextureAsync(cancelationToken));
            }
        }

        private void Convert(BitmapPalette palette, Stream output)
        {
            foreach (Color color in palette.Colors)
                ColorsHelper.WriteBgra(output, color);

            for (int i = palette.Colors.Count; i < 256; i++)
                ColorsHelper.WriteBgra(output, Colors.Transparent);
        }
    }
}