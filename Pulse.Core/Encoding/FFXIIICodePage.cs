using System;
using System.Collections.Generic;
using System.Xml;

namespace Pulse.Core
{
    public sealed class FFXIIICodePage
    {
        public readonly char[] Chars;
        public readonly Dictionary<char, byte> Bytes;

        public FFXIIICodePage(char[] chars, Dictionary<char, byte> bytes)
        {
            Chars = Exceptions.CheckArgumentNull(chars, "chars");
            Bytes = Exceptions.CheckArgumentNull(bytes, "bytes");
        }

        public char this[byte b]
        {
            get
            {
                char c = Chars[b];
                if (c == '\0')
                    throw new ArgumentOutOfRangeException("b", b, "Символ соответствующий заданному байту не задан.");
                return c;
            }
        }

        public byte this[char c]
        {
            get { return Bytes[c]; }
        }

        public char? TryGetChar(byte b)
        {
            char c = Chars[b];
            if (c == '\0')
                return null;
            return c;
        }

        public byte? TryGetByte(char c)
        {
            byte b;
            if (Bytes.TryGetValue(c, out b))
                return b;
            return null;
        }
    }
}