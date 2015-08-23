using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Pulse.DirectX
{
    public class Dx11Device : IDisposable
    {
        protected Device _device;

        public Device Device => _device;

        protected Dx11Device()
        {
        }

        public Dx11Device(Adapter adapter)
        {
            _device = new Device(adapter, GetDeviceCreationFlags(), FeatureLevel.Level_11_0);
        }

        public virtual void Dispose()
        {
            _device?.Dispose();
        }

        protected static DeviceCreationFlags GetDeviceCreationFlags()
        {
            DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;
#if (DEBUG)
            flags |= DeviceCreationFlags.Debug;
#endif
            return flags;
        }

        public static explicit operator Device(Dx11Device self)
        {
            return self?._device;
        }

        public static Dx11Device CreateDefaultAdapter()
        {
            using (Factory1 factory = new Factory1())
            using (Adapter adapter = factory.GetAdapter(0))
                return new Dx11Device(adapter);
        }
    }
}