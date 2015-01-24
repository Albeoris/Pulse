using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public sealed class BitmapGLTextureReader : GLTextureReader
    {
        private readonly Bitmap _bitmap;
        private readonly int _width;
        private readonly int _height;
        private readonly PixelFormatDescriptor _format;

        public BitmapGLTextureReader(Bitmap bitmap, int width, int height, PixelFormatDescriptor format)
        {
            _bitmap = bitmap;
            _width = width;
            _height = height;
            _format = format;
        }

        public override async Task<GLTexture> ReadTextureAsync(CancellationToken cancelationToken)
        {
            if (cancelationToken.IsCancellationRequested)
                return RaiseTextureReaded(null);

            BitmapData bmdata = _bitmap.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, _format);
            using (DisposableAction unlocker = new DisposableAction(() => _bitmap.UnlockBits(bmdata)))
            {
                GLTexture texture;
                using (GLService.AcquireContext())
                    texture = new GLTexture(GL.GenTexture(), _width, _height, _format);

                using (DisposableAction insurance = new DisposableAction(texture.Dispose))
                {
                    if (cancelationToken.IsCancellationRequested)
                        return RaiseTextureReaded(null);

                    using (GLService.AcquireContext())
                    {
                        GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, _format, bmdata.Width, bmdata.Height, 0, _format, _format, bmdata.Scan0);
                    }

                    //ErrorCode error = GL.GetError();
                    //if (error != ErrorCode.NoError)
                    //    throw new ArgumentException("Error building TexImage. GL Error: " + error);

                    _bitmap.UnlockBits(bmdata);
                    unlocker.Cancel();

                    if (cancelationToken.IsCancellationRequested)
                        return RaiseTextureReaded(null);

                    using (GLService.AcquireContext())
                    {
                        GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    }

                    insurance.Cancel();
                }
                return RaiseTextureReaded(texture);
            }
        }
    }
}