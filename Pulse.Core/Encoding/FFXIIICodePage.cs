using System;
using System.Collections.Generic;
using System.Xml;

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

        public char this[short b]
        {
            get
            {
                if (b == 324)
                    return 'Ⅷ';

                char c = Chars[b];
                if (c == '\0')
                    throw new ArgumentOutOfRangeException("b", b, "Символ соответствующий заданному байту не задан.");
                return c;
            }
        }

        public short this[char c]
        {
            get
            {
                return TryGetByte(c) ?? Codes['#'];
            }
        }

        public char? TryGetChar(byte b)
        {
            char c = Chars[b];
            if (c == '\0')
                return null;
            return c;
        }

        public short? TryGetByte(char c)
        {
            short b;
            if (Codes.TryGetValue(c, out b))
                return b;
            return null;
        }
    }
}