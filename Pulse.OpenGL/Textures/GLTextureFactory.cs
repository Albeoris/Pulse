using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public static class GLTextureFactory
    {
        public static GLTexture FromImageFile(string filePath)
        {
            ImageFileGLTextureReader reader = new ImageFileGLTextureReader(filePath);
            return reader.ReadTexture();
        }

        public static GLTexture FromBitmapPalettes(params BitmapPalette[] palettes)
        {
            BitmapPalettesGLTextureReader reader = new BitmapPalettesGLTextureReader(palettes);
            return reader.ReadTexture();
        }

        public static GLTexture FromStream(Stream input, int width, int height, PixelFormatDescriptor format)
        {
            StreamGLTextureReader reader = new StreamGLTextureReader(input, width, height, format);
            return reader.ReadTexture();
        }

        public static GLTexture FromBitmap(Bitmap bitmap, int width, int height, PixelFormatDescriptor format)
        {
            BitmapGLTextureReader reader = new BitmapGLTextureReader(bitmap, width, height, format);
            return reader.ReadTexture();
        }

        public static async Task<GLTexture> FromImageFile(CancellationToken cancelationToken, string filePath)
        {
            ImageFileGLTextureReader reader = new ImageFileGLTextureReader(filePath);
            return await reader.ReadTextureAsync(cancelationToken);
        }

        public static async Task<GLTexture> FromBitmapPalettesAsync(CancellationToken cancelationToken, params BitmapPalette[] palettes)
        {
            BitmapPalettesGLTextureReader reader = new BitmapPalettesGLTextureReader(palettes);
            return await reader.ReadTextureAsync(cancelationToken);
        }

        public static async Task<GLTexture> FromStreamAsync(CancellationToken cancelationToken, Stream input, int width, int height, PixelFormatDescriptor format)
        {
            StreamGLTextureReader reader = new StreamGLTextureReader(input, width, height, format);
            return await reader.ReadTextureAsync(cancelationToken);
        }

        public static async Task<GLTexture> FromBitmapAsync(CancellationToken cancelationToken, Bitmap bitmap, int width, int height, PixelFormatDescriptor format)
        {
            BitmapGLTextureReader reader = new BitmapGLTextureReader(bitmap, width, height, format);
            return await reader.ReadTextureAsync(cancelationToken);
        }

        public static GLTexture HorizontalJoin(GLTexture left, GLTexture right)
        {
            Exceptions.CheckArgumentNull(left, "left");
            Exceptions.CheckArgumentNull(right, "right");

            if (left.Height != right.Height) throw new ArgumentException("height");
            if (!left.PixelFormat.Equals(right.PixelFormat)) throw new ArgumentException("pixelFormat");

            int height = left.Height;
            PixelFormatDescriptor format = left.PixelFormat;

            GLTexture texture = null;
            try
            {
                using (GLService.AcquireContext())
                {
                    texture = new GLTexture(GL.GenTexture(), left.Width + right.Width, height, format);
                    GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, format, texture.Width, texture.Height, 0, format, format, IntPtr.Zero);

                    if (GLService.CheckVersion(4, 3))
                    {
                        GL.CopyImageSubData(left.Id, ImageTarget.Texture2D, 0, 0, 0, 0, texture.Id, ImageTarget.Texture2D, 0, 0, 0, 0, left.Width, left.Height, 1);
                        GL.CopyImageSubData(right.Id, ImageTarget.Texture2D, 0, 0, 0, 0, texture.Id, ImageTarget.Texture2D, 0, left.Width, 0, 0, right.Width, right.Height, 1);
                    }
                    else
                    {
                        using (GLFramebuffer framebuffer = GLFramebuffer.Create())
                        {
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer.Id);
                            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, texture.Id, 0);
                            
                            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, left.Id, 0);
                            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
                            GL.BlitFramebuffer(0, 0, left.Width, height, 0, 0, left.Width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, right.Id, 0);
                            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
                            GL.BlitFramebuffer(0, 0, left.Width, height, left.Width, 0, left.Width * 2, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
                        }
                    }

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                }
            }
            catch
            {
                texture.SafeDispose();
                throw;
            }

            return texture;
        }

        public static void HorizontalSplit(GLTexture layer, out GLTexture leftTexture, out GLTexture rightTexture)
        {
            Exceptions.CheckArgumentNull(layer, "layer");
            if (layer.Width % 2 != 0) throw new ArgumentException("layer.Width");

            int width = layer.Width / 2; 
            int height = layer.Height;
            PixelFormatDescriptor format = layer.PixelFormat;

            leftTexture = null;
            rightTexture = null;

            try
            {
                using (GLService.AcquireContext())
                {
                    leftTexture = new GLTexture(GL.GenTexture(), width, height, format);
                    rightTexture = new GLTexture(GL.GenTexture(), width, height, format);

                    foreach (GLTexture texture in new[] { leftTexture, rightTexture })
                    {
                        GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, format, format, IntPtr.Zero);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    }

                    if (GLService.CheckVersion(4, 3))
                    {
                        GL.CopyImageSubData(layer.Id, ImageTarget.Texture2D, 0, 0, 0, 0, leftTexture.Id, ImageTarget.Texture2D, 0, 0, 0, 0, width, height, 1);
                        GL.CopyImageSubData(layer.Id, ImageTarget.Texture2D, 0, width, 0, 0, rightTexture.Id, ImageTarget.Texture2D, 0, 0, 0, 0, width, height, 1);
                    }
                    else
                    {
                        using (GLFramebuffer framebuffer = GLFramebuffer.Create())
                        {
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer.Id);
                            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, layer.Id, 0);
                            
                            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, leftTexture.Id, 0);
                            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
                            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, rightTexture.Id, 0);
                            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
                            GL.BlitFramebuffer(width, 0, width * 2, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
                        }
                    }
                }
            }
            catch
            {
                leftTexture.SafeDispose();
                rightTexture.SafeDispose();
                throw;
            }
        }
    }
}