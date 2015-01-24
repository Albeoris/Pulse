using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.OpenGL
{
    public sealed class ImageFileGLTextureReader : GLTextureReader
    {
        private readonly string _filePath;

        public ImageFileGLTextureReader(string filePath)
        {
            _filePath = filePath;
        }

        public override async Task<GLTexture> ReadTextureAsync(CancellationToken cancelationToken)
        {
            if (cancelationToken.IsCancellationRequested)
                return RaiseTextureReaded(null);

            using (Bitmap bitmap = new Bitmap(_filePath))
            {
                BitmapGLTextureReader bitmapReader = new BitmapGLTextureReader(bitmap, bitmap.Width, bitmap.Height, bitmap.PixelFormat);
                return RaiseTextureReaded(await bitmapReader.ReadTextureAsync(cancelationToken));
            }
        }
    }
}