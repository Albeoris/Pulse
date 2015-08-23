using System;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device1 = SharpDX.Direct3D10.Device1;

namespace Pulse.DirectX
{
    public class Dx10Device : IDisposable
    {
        protected Device1 _device;

        public Device1 Device => _device;

        protected Dx10Device()
        {
        }

        public Dx10Device(Adapter adapter)
        {
            _device = new Device1(adapter, GetDeviceCreationFlags(), FeatureLevel.Level_10_0);
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

        public static explicit operator Device1(Dx10Device self)
        {
            return self?._device;
        }

        public static Dx10Device CreateDefaultAdapter()
        {
            using (Factory1 factory = new Factory1())
            using (Adapter adapter = factory.GetAdapter(0))
                return new Dx10Device(adapter);
        }
    }
}