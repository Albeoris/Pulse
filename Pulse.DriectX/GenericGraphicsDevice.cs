using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;

namespace Pulse.DirectX
{
    public class GenericGraphicsDevice : GraphicsDevice
    {
        public GenericGraphicsDevice(DriverType type, DeviceCreationFlags flags = DeviceCreationFlags.None, params FeatureLevel[] featureLevels)
            : base(type, flags, featureLevels)
        {
        }

        public GenericGraphicsDevice(GraphicsAdapter adapter, DeviceCreationFlags flags = DeviceCreationFlags.None, params FeatureLevel[] featureLevels)
            : base(adapter, flags, featureLevels)
        {
        }

        public GenericGraphicsDevice(Device existingDevice, GraphicsAdapter adapter = null)
            : base(existingDevice, adapter)
        {
        }

        public GenericGraphicsDevice(GraphicsDevice mainDevice, DeviceContext deferredContext)
            : base(mainDevice, deferredContext)
        {
        }
    }
}