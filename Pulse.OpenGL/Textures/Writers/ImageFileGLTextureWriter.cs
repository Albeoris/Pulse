using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Pulse.OpenGL
{
    public sealed class ImageFileGLTextureWriter : IGLTextureWriter
    {
        private readonly GLTexture _texture;
        private readonly string _filePath;
        private readonly ImageFormat _imageFormat;

        public ImageFileGLTextureWriter(GLTexture texture, string filePath, ImageFormat imageFormat)
        {
            _texture = texture;
            _filePath = filePath;
            _imageFormat = imageFormat;
        }

        public void Write()
        {
            using (Bitmap bmp = new Bitmap(_texture.Width, _texture.Height, _texture.PixelFormat))
            {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, _texture.PixelFormat);
                using (GLService.AcquireContext())
                {
                    GL.BindTexture(TextureTarget.Texture2D, _texture.Id);
                    GL.GetTexImage(TextureTarget.Texture2D, 0, _texture.PixelFormat, _texture.PixelFormat, data.Scan0);
                }
                bmp.UnlockBits(data);
                bmp.Save(_filePath, _imageFormat);
            }
        }
    }
}