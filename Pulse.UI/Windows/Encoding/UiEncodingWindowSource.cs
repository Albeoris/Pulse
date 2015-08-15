using System.Collections.Concurrent;
using Pulse.DirectX;
using Pulse.FS;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingWindowSource
    {
        public readonly DxTexture Texture;
        public readonly WflContent Info;
        public readonly char[] Chars;
        public readonly ConcurrentDictionary<char, short> Codes;

        public UiEncodingWindowSource(string displayName, DxTexture texture, WflContent info, char[] chars, ConcurrentDictionary<char, short> codes)
        {
            DisplayName = displayName;
            Texture = texture;
            Info = info;
            Chars = chars;
            Codes = codes;
        }

        public string DisplayName { get; private set; }
    }
}