using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;
using Pulse.OpenGL;

namespace Pulse.UI
{
    public class UiGlViewport : UiScrollViewer
    {
        public readonly UiScrollableGlControl GlControl;

        public AutoResetEvent DrawEvent;
        private Action _drawMethod;

        public UiGlViewport(Action drawMethod)
        {
            _drawMethod = Exceptions.CheckArgumentNull(drawMethod, "drawMethod");

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            GlControl = new UiScrollableGlControl();
            {
                GlControl.ClipToBounds = true;
                GlControl.Control.Load += OnGLControlElementLoaded;
                GlControl.Control.Resize += OnGLControlElementResize;
            }

            Content = GlControl;
        }

        private void OnGLControlElementLoaded(object sender, EventArgs e)
        {
            GLService.SubscribeControl(GlControl);
            DrawEvent = GLService.RegisterDrawMethod(_drawMethod);
        }

        private void OnGLControlElementResize(object sender, EventArgs e)
        {
            ConfigGlControl();
        }

        private void ConfigGlControl()
        {
            if (!GlControl.IsLoaded)
                return;

            using (GlControl.AcquireContext())
            {
                GL.ClearColor(Color4.Black);

                int w = Math.Max(1, GlControl.Control.Width);
                int h = Math.Max(1, GlControl.Control.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, w, h, 0, -1, 1);
                GL.Viewport(0, 0, w, h);

                DrawEvent.NullSafeSet();
            }
        }

        public IDisposable AcquireContext()
        {
            return GlControl.AcquireContext();
        }

        public void SwapBuffers()
        {
            GlControl.SwapBuffers();
        }

        public void Reconfig()
        {
            ConfigGlControl();
        }
    }
}