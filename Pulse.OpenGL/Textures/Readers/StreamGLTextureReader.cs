using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public sealed class StreamGLTextureReader : GLTextureReader
    {
        private readonly Stream _input;
        private readonly int _width;
        private readonly int _height;
        private readonly PixelFormatDescriptor _format;

        public StreamGLTextureReader(Stream input, int width, int height, PixelFormatDescriptor format)
        {
            _input = input;
            _width = width;
            _height = height;
            _format = format;
        }

        public override async Task<GLTexture> ReadTextureAsync(CancellationToken cancelationToken)
        {
            if (cancelationToken.IsCancellationRequested)
                return RaiseTextureReaded(null);

            using (Bitmap bitmap = new Bitmap(_width, _height, _format))
            {
                if (cancelationToken.IsCancellationRequested)
                    return RaiseTextureReaded(null);

                BitmapData bmdata = bitmap.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, _format);
                using (new DisposableAction(() => bitmap.UnlockBits(bmdata)))
                {
                    if (cancelationToken.IsCancellationRequested)
                        return RaiseTextureReaded(null);

                    using (UnmanagedMemoryStream output = bmdata.Scan0.OpenStream(bmdata.Stride * bmdata.Height, FileAccess.Write))
                        _input.CopyTo(output);
                }

                if (cancelationToken.IsCancellationRequested)
                    return RaiseTextureReaded(null);

                BitmapGLTextureReader bitmapReader = new BitmapGLTextureReader(bitmap, _width, _height, _format);
                return RaiseTextureReaded(await bitmapReader.ReadTextureAsync(cancelationToken));
            }
        }
    }
}