using System;
using System.Windows.Media;
using Pulse.Core;
using SharpDX.DXGI;
using SharpDX.Toolkit.Graphics;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;

namespace Pulse.DirectX
{
    public sealed class RenderContainer : IDisposable
    {
        public readonly Dx10Device Device10;
        public readonly Dx11ChainedDevice Device11;
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

                using (Factory1 factory = new Factory1())
                using (Adapter adapter = factory.GetAdapter(0))
                {
                    Device11 = new Dx11ChainedDevice(adapter, _swapChainDescription);
                    Device10 = new Dx10Device(adapter);
                }

                GraphicsDevice = new GenericGraphicsDevice(Device11.Device);
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
            Device10?.Dispose();
            Device11?.Dispose();
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

            BackBuffer = new BackBuffer(Device10, Device11);
            DepthBuffer = new DepthBuffer(Device11, width, height);

            if (backgroundColor != null)
                BackBuffer.BackgroundColor = backgroundColor.Value;

            SetViewport(width, height);
            SetTargets();

            Reseted?.Invoke(this);
        }

        private void ResizeBuffers(int width, int height)
        {
            Device11.SwapChain.ResizeBuffers(_swapChainDescription.BufferCount, width, height, _swapChainDescription.ModeDescription.Format, _swapChainDescription.Flags);
        }

        private void SetViewport(int width, int height)
        {
            Device11.Device.ImmediateContext.Rasterizer.SetViewport(0, 0, width, height);
        }

        private void SetTargets()
        {
            Device11.Device.ImmediateContext.OutputMerger.SetTargets(DepthBuffer.View, BackBuffer.View);
        }
    }
}