using System;
using System.Windows.Media;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using Factory = SharpDX.Direct2D1.Factory;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using Resource = SharpDX.Direct3D11.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Pulse.DirectX
{
    public sealed class BackBuffer : IDisposable
    {
        private readonly Dx10Device _device10;
        private readonly Dx11ChainedDevice _device11;
        private readonly Factory _factory2D;

        private Texture2D _backBuffer;
        private RenderTargetView _renderView;
        private Surface _surface;
        private RenderTarget _renderTarget2D;
        private Texture2D _textureD3D11;

        public Texture2D Buffer => _backBuffer;
        public RenderTargetView View => _renderView;
        public Factory Factory2D => _factory2D;
        public Surface Surface => _surface;
        public RenderTarget Target2D => _renderTarget2D;

        public Color BackgroundColor { get; set; } = Colors.LightGray;

        public BackBuffer(Dx10Device device10, Dx11ChainedDevice device11)
        {
            try
            {
                _device10 = device10;
                _device11 = device11;

                _backBuffer = Resource.FromSwapChain<Texture2D>(device11.SwapChain, 0);
                _renderView = new RenderTargetView(device11.Device, _backBuffer);

                Texture2DDescription descriptor = _backBuffer.Description;
                {
                    descriptor.MipLevels = 1;
                    descriptor.ArraySize = 1;
                    descriptor.Format = Format.B8G8R8A8_UNorm;
                    descriptor.SampleDescription = new SampleDescription(1, 0);
                    descriptor.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
                    descriptor.BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource;
                    descriptor.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
                    descriptor.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.SharedKeyedmutex;
                };

                _textureD3D11 = new Texture2D(device11.Device, descriptor);

                _factory2D = new Factory(FactoryType.MultiThreaded);

                using (SharpDX.DXGI.Resource sharedResource = _textureD3D11.QueryInterface<SharpDX.DXGI.Resource>())
                using (SharpDX.Direct3D10.Texture2D backBuffer10 = device10.Device.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(sharedResource.SharedHandle))
                {
                    _surface = backBuffer10.QueryInterface<Surface>();
                    _renderTarget2D = new RenderTarget(_factory2D, _surface, GetRenderTargetProperties());
                }
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
            _textureD3D11.Dispose();
            _renderView?.Dispose();
            _backBuffer?.Dispose();
        }

        public void Clear()
        {
            if (_renderView == null)
                return;

            _device11.Device.ImmediateContext.ClearRenderTargetView(_renderView, BackgroundColor.ToColor4());
        }

        private static RenderTargetProperties GetRenderTargetProperties()
        {
            PixelFormat format = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied);
            return new RenderTargetProperties(format) { MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_10, };
        }
    }
}