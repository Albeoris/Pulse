using System;
using System.IO;
using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextReference
    {
        public const int MaxTagLength = 32;
        public readonly String Content;

        public const Byte Mark = 0x24; // $
        public const Byte OpenBracket = 0x28; // (
        public const Byte CloseBracket = 0x29; // )

        public Int32 SizeInBytes => Content?.Length ?? 0;
        public Int32 SizeInChars => Content?.Length ?? 0;

        public FFXIIITextReference(String content)
        {
            Content = content;
        }

        public int Write(byte[] bytes, ref int offset)
        {
            int count = Encoding.ASCII.GetBytes(Content, 0, Content.Length, bytes, offset);
            offset += count;
            return count;
        }

        public int Write(char[] chars, ref int offset)
        {
            Content.CopyTo(0, chars, offset, Content.Length);
            offset += Content.Length;
            return Content.Length;
        }

        public static FFXIIITextReference TryRead(byte[] bytes, ref int offset, ref int left)
        {
            if (left < 4 || bytes[offset] != Mark || bytes[offset + 1] != OpenBracket)
                return null;

            byte[] result = new byte[MaxTagLength];

            int index;
            for (index = 0; index < MaxTagLength; index++)
            {
                left--;
                byte value = bytes[offset++];
                result[index] = value;
                if (value == CloseBracket)
                    break;
            }

            if (index >= MaxTagLength)
                throw new InvalidDataException();

            String content = Encoding.ASCII.GetString(result, 0, index + 1);
            return new FFXIIITextReference(content);
        }

        public static FFXIIITextReference TryRead(char[] chars, ref int offset, ref int left)
        {
            if (left < 4 || chars[offset] != '$' || chars[offset + 1] != '(')
                return null;

            char[] result = new char[MaxTagLength];

            int index;
            for (index = 0; index < MaxTagLength; index++)
            {
                left--;
                char value = chars[offset++];
                result[index] = value;
                if (value == ')')
                    break;
            }

            if (index >= MaxTagLength)
                throw new InvalidDataException();

            String content = new string(result, 0, index + 1);
            return new FFXIIITextReference(content);
        }

        public override string ToString()
        {
            return Content;
        }
    }
}