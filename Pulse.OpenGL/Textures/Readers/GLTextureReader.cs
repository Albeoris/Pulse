using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.OpenGL
{
    public abstract class GLTextureReader : IDisposable
    {
        public event Action<GLTexture> TextureReaded;
        public abstract Task<GLTexture> ReadTextureAsync(CancellationToken cancelationToken);

        public GLTexture ReadTexture()
        {
            return ReadTextureAsync(CancellationToken.None).Result;
        }

        protected GLTexture RaiseTextureReaded(GLTexture texture)
        {
            Action<GLTexture> h = TextureReaded;
            if (h != null)
                h(texture);
            return texture;
        }

        public virtual void Dispose()
        {
        }
    }
}