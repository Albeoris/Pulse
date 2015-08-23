using System;
using System.Collections.Generic;
using Pulse.Core;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Resource = SharpDX.Direct3D11.Resource;

namespace Pulse.DirectX
{
    public sealed class DxTexture : IDisposable
    {
        private readonly Dictionary<IntPtr, IntPtr> _sharedViews = new Dictionary<IntPtr, IntPtr>(1);

        public readonly Resource Texture;
        public readonly Texture2DDescription Descriptor2D;
        public bool IsCube { get; private set; }

        public DxTexture(Resource texture, Texture2DDescription descriptor2D)
        {
            Texture = texture;
            Descriptor2D = descriptor2D;
            IsCube = descriptor2D.ArraySize == 6;
        }

        public void Dispose()
        {
            DisposeSharedViews();
            Texture.Dispose();
        }

        private void DisposeSharedViews()
        {
            try
            {
                lock (_sharedViews)
                {
                    foreach (IntPtr viewPointer in _sharedViews.Values)
                        new ShaderResourceView(viewPointer).Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void Draw(Device device, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, float layerDepth)
        {
            ShaderResourceView shaderView = GetShaderResourceView(device);
            spriteBatch.Draw(shaderView,
                position,
                sourceRectangle,
                new Color(0xff, 0xff, 0xff, 0xff),
                0,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None, layerDepth);
        }

        private ShaderResourceView GetShaderResourceView(Device device)
        {
            lock (_sharedViews)
            {
                IntPtr viewPointer;
                if (_sharedViews.TryGetValue(device.NativePointer, out viewPointer))
                    return new ShaderResourceView(viewPointer);

                using (SharpDX.DXGI.Resource resource = Texture.QueryInterface<SharpDX.DXGI.Resource>())
                using (Resource sharedResource = device.OpenSharedResource<Resource>(resource.SharedHandle))
                {
                    ShaderResourceView shaderView = new ShaderResourceView(device, sharedResource);
                    _sharedViews.Add(device.NativePointer, shaderView.NativePointer);
                    return shaderView;
                }
            }
        }

        public void Draw(Device device, SpriteBatch spriteBatch, Vector2 position, Rectangle sourceRectangle, float layerDepth, Rectangle viewport)
        {
            position.X -= viewport.Left;
            if (position.X < 0)
            {
                sourceRectangle.Left -= (int)position.X;
                if (sourceRectangle.Width < 1)
                    return;
                position.X = 0;
            }

            int ox = viewport.Width - (int)(position.X + sourceRectangle.Width);
            if (ox < 0)
            {
                sourceRectangle.Right += ox;
                if (sourceRectangle.Width < 1)
                    return;
            }

            position.Y -= viewport.Top;
            if (position.Y < 0)
            {
                sourceRectangle.Top -= (int)position.Y;
                if (sourceRectangle.Height < 1)
                    return;
                position.Y = 0;
            }

            int oy = viewport.Height - (int)(position.Y + sourceRectangle.Height );
            if (oy < 0)
            {
                sourceRectangle.Bottom += oy;
                if (sourceRectangle.Height < 1)
                    return;
            }

            Draw(device, spriteBatch, position, sourceRectangle, layerDepth);
        }
    }
}