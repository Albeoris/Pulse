using System;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public sealed class GLTexture : IDisposable
    {
        public readonly int Id;
        public readonly int Width;
        public readonly int Height;
        public readonly PixelFormatDescriptor PixelFormat;

        public GLTexture(int id, int width, int height, PixelFormatDescriptor pixelFormat)
        {
            Id = id;
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
        }

        public void Dispose()
        {
            using (GLService.AcquireContext())
                GL.DeleteTexture(Id);
        }

        public void ToImageFile(string filePath)
        {
            Exceptions.CheckArgumentNullOrEmprty(filePath, "filePath");

            ImageFormat imageFormat = PixelFormat.GetOptimalImageFormat();
            filePath = Path.ChangeExtension(filePath, imageFormat.ToString().ToLower());

            ImageFileGLTextureWriter writer = new ImageFileGLTextureWriter(this, filePath, imageFormat);
            writer.Write();
        }

        public byte[] GetManagedPixelsArray(PixelFormatDescriptor format)
        {
            byte[] result = new byte[Width * Height * format.BytesPerPixel]; 
            
            using (GLService.AcquireContext())
            {
                GL.BindTexture(TextureTarget.Texture2D, Id);
                GL.GetTexImage(TextureTarget.Texture2D, 0, format, format, result);
                return result;
            }
        }

        public SafeUnmanagedArray GetUnmanagedPixelsArray(PixelFormatDescriptor format)
        {
            SafeUnmanagedArray result = new SafeUnmanagedArray(Width * Height * format.BytesPerPixel);
            using (GLService.AcquireContext()) 
            using (DisposableAction insurance = new DisposableAction(result.Dispose))
            {
                GL.BindTexture(TextureTarget.Texture2D, Id);
                GL.GetTexImage(TextureTarget.Texture2D, 0, format, format, result.DangerousGetHandle());
                
                insurance.Cancel();
            }
            return result;
        }

        public void Draw(float x, float y, float z)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0); GL.Vertex3(x, y + Height, z);
            GL.TexCoord2(0, 1); GL.Vertex3(x, y, z);
            GL.TexCoord2(1, 1); GL.Vertex3(x + Width, y, z);
            GL.TexCoord2(1, 0); GL.Vertex3(x + Width, y + Height, z);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public void Draw(float x, float y, float z, float ox, float oy, float w, float h)
        {
            float tx = ox / Width;
            float ty = 1 - oy / Height;
            float tw = w / Width;
            float th = h / Height;
            ty -= th;

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(tx, ty + th);           GL.Vertex3(x, y, z);
            GL.TexCoord2(tx + tw, ty + th);      GL.Vertex3(x + w, y, z);
            GL.TexCoord2(tx + tw, ty); GL.Vertex3(x + w, y + h, z);
            GL.TexCoord2(tx, ty);      GL.Vertex3(x, y + h, z);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}