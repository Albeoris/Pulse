using SharpDX;
using Color = System.Windows.Media.Color;

namespace Pulse.DirectX
{
    public static class DxColorsHelper
    {
        public static Color4 ToColor4(this Color color)
        {
            const float max = byte.MaxValue;
            return new Color4(color.R / max, color.G / max, color.B / max, color.A / max);
        }
    }
}