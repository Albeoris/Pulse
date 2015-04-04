using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;
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
    }
}