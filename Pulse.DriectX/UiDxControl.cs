using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Pulse.Core;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Toolkit.Graphics;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Pulse.DirectX
{
    public class UiDxControl : WindowsFormsHost
    {
        public delegate void DrawSpritesDelegate(Device device, SpriteBatch spriteBatch, Rectangle clipRectangle);

        public delegate void DrawPrimitivesDelegate(Device device, RenderTarget target2D, Rectangle clipRectangle);

        public readonly RenderControl Control;
        public readonly RenderContainer RenderContainer;

        public event DrawSpritesDelegate DrawSprites;
        public event DrawPrimitivesDelegate DrawPrimitives;

        private readonly Semaphore _semaphore = new Semaphore(2, 2);

        public UiDxControl()
        {
            Control = new RenderControl();

            SwapChainDescription swapChainDescription = CreateSwapChainDescription();
            RenderContainer = new RenderContainer(swapChainDescription, Control);

            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            Child = Control;
            Control.Paint += OnRenderControlPaint;
        }

        [HandleProcessCorruptedStateExceptions]
        private void OnRenderControlPaint(object sender, PaintEventArgs e)
        {
            if (!_semaphore.WaitOne(0, false))
                return;

            try
            {
                lock (_semaphore)
                {
                    RenderContainer.DepthBuffer.Clear();
                    RenderContainer.BackBuffer.Clear();

                    Rectangle clipRectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);

                    DrawSprites?.Invoke(RenderContainer.Device11.Device, RenderContainer.SpriteBatch, clipRectangle);
                    DrawPrimitives?.Invoke(RenderContainer.Device11.Device, RenderContainer.BackBuffer.Target2D, clipRectangle);

                    RenderContainer.Device11.SwapChain.Present(1, PresentFlags.None);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private SwapChainDescription CreateSwapChainDescription()
        {
            ModeDescription bufferDescription = CreateModeDescription();

            return new SwapChainDescription()
            {
                ModeDescription = bufferDescription,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = Control.Handle,
                IsWindowed = true
            };
        }

        private ModeDescription CreateModeDescription()
        {
            return new ModeDescription
            {
                RefreshRate = new Rational(60, 1),
                Format = Format.B8G8R8A8_UNorm
            };
        }
    }
}