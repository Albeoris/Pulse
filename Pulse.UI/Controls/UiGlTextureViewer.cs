using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;

namespace Pulse.UI
{
    public sealed class UiGlTextureViewer : UserControl
    {
        private UiGlViewport _viewport;
        private GLTexture _texture;

        public UiGlTextureViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;

            _viewport = new UiGlViewport(Draw);
            Content = _viewport;
        }

        public GLTexture Texture
        {
            get { return _texture; }
            set
            {
                if (_texture != null)
                    _texture.SafeDispose();

                if (value != null)
                    GLService.SetViewportDesiredSize(value.Width, value.Height);

                _texture = value;
                _viewport.Reconfig();
            }
        }

        private void Draw()
        {
            GLTexture texture = _texture;
            if (texture == null)
                return;

            using (_viewport.AcquireContext())
                texture.Draw(0, 0, 0);

            _viewport.SwapBuffers();
        }

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            Visibility = Visibility.Visible;
            Texture = GLTextureReader.ReadFromWpd(listing, entry);
        }
    }
}