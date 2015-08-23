using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Pulse.DirectX
{
    public sealed class Dx11ChainedDevice : Dx11Device
    {
        private readonly SwapChain _swapChain;

        public SwapChain SwapChain => _swapChain;

        public Dx11ChainedDevice(Adapter adapter, SwapChainDescription descriptor)
        {
            Device.CreateWithSwapChain(adapter, GetDeviceCreationFlags(), descriptor, out _device, out _swapChain);
        }

        public override void Dispose()
        {
            _swapChain?.Dispose();
            base.Dispose();
        }

        public static explicit operator SwapChain(Dx11ChainedDevice self)
        {
            return self?._swapChain;
        }
    }
}