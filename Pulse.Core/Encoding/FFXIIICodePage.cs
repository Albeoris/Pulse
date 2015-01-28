using System.Collections.Generic;

namespace Pulse.Core
{
    public sealed class FFXIIICodePage
    {
        public readonly char[] Chars;
        public readonly Dictionary<char, short> Codes;

        public FFXIIICodePage(char[] chars, Dictionary<char, short> bytes)
        {
            Chars = Exceptions.CheckArgumentNull(chars, "chars");
            Codes = Exceptions.CheckArgumentNull(bytes, "bytes");
        }

        public char this[short code]
        {
            get { return TryGetChar(code) ?? 'Ⅷ'; }
        }

        public short this[char ch]
        {
            get { return TryGetCode(ch) ?? Codes['#']; }
        }

        public char? TryGetChar(short code)
        {
            char c = Chars[code];
            if (c == '\0')
                return null;
            return c;
        }

        public short? TryGetCode(char ch)
        {
            short b;
            if (Codes.TryGetValue(ch, out b))
                return b;
            return null;
        }
    }
}