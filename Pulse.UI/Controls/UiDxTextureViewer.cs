using System;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;

namespace Pulse.UI
{
    public sealed class UiDxTextureViewer : UserControl
    {
        private UiDxViewport _viewport;
        private DxTexture _texture;

        public UiDxTextureViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;

            _viewport = new UiDxViewport();
            _viewport.DrawSprites += DrawSprites;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            Content = _viewport;
        }

        public DxTexture Texture
        {
            get { return _texture; }
            set
            {
                _texture?.SafeDispose();
                _texture = value;

                if (_texture != null)
                    _viewport.SetDesiredSize(_texture.Descriptor2D.Width, _texture.Descriptor2D.Height);
            }
        }

        private void DrawSprites(Device device, SpriteBatch spritebatch, Rectangle cliprectangle)
        {
            try
            {
                DxTexture texture = _texture;
                if (texture == null)
                    return;

                spritebatch.Begin();
                try
                {
                    int width = Math.Min(texture.Descriptor2D.Width, cliprectangle.Width);
                    int height = Math.Min(texture.Descriptor2D.Height, cliprectangle.Height);
                    Rectangle rectangle = new Rectangle(cliprectangle.X, cliprectangle.Y, width, height);
                    texture.Draw(device, spritebatch, Vector2.Zero, rectangle, 1.0f);
                }
                finally
                {
                    spritebatch.End();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            Visibility = Visibility.Visible;
            Texture = DxTextureReader.ReadFromWpd(listing, entry);
        }
    }
}