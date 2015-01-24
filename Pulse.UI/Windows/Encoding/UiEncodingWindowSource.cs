using System.Collections.Concurrent;
using System.Collections.Generic;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingWindowSource
    {
        public readonly GLTexture Texture;
        public readonly WflContent Info;
        public readonly char[] Chars;
        public readonly ConcurrentDictionary<char, short> Codes;

        public UiEncodingWindowSource(string displayName, GLTexture texture, WflContent info, char[] chars, ConcurrentDictionary<char, short> codes)
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