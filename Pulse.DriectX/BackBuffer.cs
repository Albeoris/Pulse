using System;
using System.Windows.Media;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Toolkit.Graphics;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using Resource = SharpDX.Direct3D11.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Color = System.Windows.Media.Color;

namespace Pulse.DirectX
{
    public sealed class BackBuffer : IDisposable
    {
        private readonly Device _device;
        private readonly Factory _factory2D;

        private Texture2D _backBuffer;
        private RenderTargetView _renderView;
        private Surface _surface;
        private RenderTarget _renderTarget2D;

        public Texture2D Buffer => _backBuffer;
        public RenderTargetView View => _renderView;
        public Factory Factory2D => _factory2D;
        public Surface Surface => _surface;
        public RenderTarget Target2D => _renderTarget2D;

        public Color BackgroundColor { get; set; } = Colors.LightGray;

        public BackBuffer(Device device, SwapChain swapChain)
        {
            try
            {
                _device = device;

                _backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);
                _renderView = new RenderTargetView(device, _backBuffer);

                _factory2D = new Factory(FactoryType.MultiThreaded);
                _surface = _backBuffer.QueryInterface<Surface>();
                _renderTarget2D = new RenderTarget(_factory2D, _surface, GetRenderTargetProperties());
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _renderTarget2D?.Dispose();
            _surface?.Dispose();
            _factory2D?.Dispose();
            _renderView?.Dispose();
            _backBuffer?.Dispose();
        }      

        public void Clear()
        {
            if (_renderView == null)
                return;

            _device.ImmediateContext.ClearRenderTargetView(_renderView, BackgroundColor.ToColor4());
        }

        private static RenderTargetProperties GetRenderTargetProperties()
        {
            PixelFormat format = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied);
            return new RenderTargetProperties(format);
        }
    }
}