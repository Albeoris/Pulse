using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Examples.TextureLoaders;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;

namespace Pulse.UI.Controls
{
    public sealed class UiGlTextureViewer : UiScrollViewer
    {
        private readonly UiScrollableGlControl _glControl;

        private AutoResetEvent _drawEvent;
        private GLTexture _texture;

        public UiGlTextureViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            _glControl = new UiScrollableGlControl();
            {
                _glControl.ClipToBounds = true;
                _glControl.Control.Load += OnGLControlElementLoaded;
                _glControl.Control.Resize += OnGLControlElementResize;
            }

            Content = _glControl;
        }

        public GLTexture Texture
        {
            get { return _texture; }
            set
            {
                if (value != null)
                    GLService.SetViewportDesiredSize(value.Width, value.Height);

                _texture = value;
                ConfigGlControl();
            }
        }

        private void OnGLControlElementLoaded(object sender, EventArgs e)
        {
            GLService.SubscribeControl(_glControl);
            _drawEvent = GLService.RegisterDrawMethod(Draw);
        }

        private void OnGLControlElementResize(object sender, EventArgs e)
        {
            ConfigGlControl();
        }

        private void ConfigGlControl()
        {
            if (!_glControl.IsLoaded)
                return;

            using (_glControl.AcquireContext())
            {
                GL.ClearColor(Color4.Black);

                int w = Math.Max(1, _glControl.Control.Width);
                int h = Math.Max(1, _glControl.Control.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, w, h, 0, -1, 1);
                GL.Viewport(0, 0, w, h);

                _drawEvent.NullSafeSet();
            }
        }

        private void Draw()
        {
            GLTexture texture = _texture;
            if (texture == null)
                return;

            using (_glControl.AcquireContext())
                texture.Draw(0, 0, 0);

            _glControl.SwapBuffers();
        }

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            using (Stream headers = listing.Accessor.ExtractHeaders())
            using (Stream content = listing.Accessor.ExtractContent())
            {
                headers.SetPosition(entry.Offset);

                SectionHeader sectionHeader = headers.ReadContent<SectionHeader>();
                TextureHeader textureHeader = headers.ReadContent<TextureHeader>();
                GtexData gtex = headers.ReadContent<GtexData>();
                if (gtex.Header.LayerCount == 0)
                    return;

                int offset = 0;
                byte[] rawData = new byte[gtex.MipMapData.Sum(d => d.Length)];
                foreach (GtexMipMapLocation mimMap in gtex.MipMapData)
                {
                    using (StreamSegment textureInput = new StreamSegment(content, mimMap.Offset, mimMap.Length, FileAccess.Read))
                    {
                        textureInput.EnsureRead(rawData, offset, mimMap.Length);
                        offset += mimMap.Length;
                    }
                }

                using (GLService.AcquireContext())
                    Texture = ImageDDS.LoadFromStream(rawData, gtex);

                Visibility = Visibility.Visible;
            }
        }
    }
}