using System;
using OpenTK.Graphics.OpenGL;

namespace Pulse.OpenGL
{
    public sealed class GLTexture : IDisposable
    {
        public readonly int Id;
        public readonly int Width;
        public readonly int Height;
        public readonly PixelFormatDescriptor PixelFormat;
        public readonly TextureTarget Dimension;

        public GLTexture(int id, int width, int height, PixelFormatDescriptor pixelFormat, TextureTarget dimension)
        {
            Id = id;
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
            Dimension = dimension;
        }

        public void Dispose()
        {
            using (GLService.AcquireContext())
                GL.DeleteTexture(Id);
        }

        public void Draw(float x, float y, float z)
        {
            if (Dimension != TextureTarget.Texture2D) // TODO
                return;

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(Dimension, Id);
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
            if (Dimension != TextureTarget.Texture2D) // TODO
                return;

            float tx = ox / Width;
            float ty = 1 - oy / Height;
            float tw = w / Width;
            float th = h / Height;
            ty -= th;

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(Dimension, Id);
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