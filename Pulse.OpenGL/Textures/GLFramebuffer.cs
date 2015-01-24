using System;
using OpenTK.Graphics.OpenGL;

namespace Pulse.OpenGL
{
    public sealed class GLFramebuffer : IDisposable
    {
        public static GLFramebuffer Create()
        {
            return new GLFramebuffer(GL.GenFramebuffer());
        }

        public readonly int Id;

        public GLFramebuffer(int id)
        {
            Id = id;
        }

        public void Dispose()
        {
            using (GLService.AcquireContext())
                GL.DeleteFramebuffer(Id);
        }
    }
}