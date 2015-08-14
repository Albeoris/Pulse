using System;
using Pulse.Core;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Toolkit.Graphics;
using SharpDX.Windows;
using Color = System.Windows.Media.Color;
using Device = SharpDX.Direct3D11.Device;

namespace Pulse.DirectX
{
    public sealed class RenderContainer : IDisposable
    {
        public readonly Device Device;
        public readonly SwapChain SwapChain;
        public readonly GenericGraphicsDevice GraphicsDevice;
        public readonly SpriteBatch SpriteBatch;

        public BackBuffer BackBuffer { get; private set; }
        public DepthBuffer DepthBuffer { get; private set; }

        private SwapChainDescription _swapChainDescription;

        public event Action<RenderContainer> Reseted;

        public RenderContainer(SwapChainDescription swapChainDescription, RenderControl control)
        {
            try
            {
                _swapChainDescription = swapChainDescription;

                CreateDeviceAndSwapChain(out Device, out SwapChain);

                GraphicsDevice = new GenericGraphicsDevice(Device);
                SpriteBatch = new SpriteBatch(GraphicsDevice);

                Reset(control.Width, control.Height);

                control.Resize += OnRenderControlResize;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            SpriteBatch?.Dispose();
            BackBuffer?.Dispose();
            DepthBuffer?.Dispose();
            GraphicsDevice?.Dispose();
            SwapChain?.Dispose();
            Device?.Dispose();
        }

        private void CreateDeviceAndSwapChain(out Device device, out SwapChain swapChain)
        {
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, _swapChainDescription, out device, out swapChain);
        }

        private void OnRenderControlResize(object sender, EventArgs e)
        {
            try
            {
                RenderControl control = (RenderControl)sender;
                int width = Math.Max(1, control.Width);
                int height = Math.Max(1, control.Height);
                Reset(width, height);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void Reset(int width, int height)
        {
            Color? backgroundColor = BackBuffer?.BackgroundColor;

            BackBuffer?.Dispose();
            DepthBuffer?.Dispose();
            
            ResizeBuffers(width, height);

            BackBuffer = new BackBuffer(Device, SwapChain);
            DepthBuffer = new DepthBuffer(Device, width, height);

            if (backgroundColor != null)
                BackBuffer.BackgroundColor = backgroundColor.Value;

            SetViewport(width, height);
            SetTargets();

            Reseted?.Invoke(this);
        }

        private void ResizeBuffers(int width, int height)
        {
            SwapChain.ResizeBuffers(_swapChainDescription.BufferCount, width, height, _swapChainDescription.ModeDescription.Format, _swapChainDescription.Flags);
        }

        private void SetViewport(int width, int height)
        {
            Device.ImmediateContext.Rasterizer.SetViewport(0, 0, width, height);
        }

        private void SetTargets()
        {
            Device.ImmediateContext.OutputMerger.SetTargets(DepthBuffer.View, BackBuffer.View);
        }
    }
}