using System;
using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextDecoder
    {
        private readonly FFXIIICodePage _codepage;

        public FFXIIITextDecoder(FFXIIICodePage codepage)
        {
            _codepage = Exceptions.CheckArgumentNull(codepage, "codepage");
        }

        public int GetMaxCharCount(int byteCount)
        {
            return byteCount * FFXIIITextTag.MaxTagLength;
        }

        public int GetCharCount(byte[] bytes, int index, int count)
        {
            int result = 0;

            char[] buff = new char[FFXIIITextTag.MaxTagLength];
            while (count > 0)
            {
                FFXIIITextTag tag = FFXIIITextTag.TryRead(bytes, ref index, ref count);
                if (tag != null)
                {
                    int offset = 0;
                    result += tag.Write(buff, ref offset);
                }
                else
                {
                    byte value = bytes[index++];
                    count--;

                    if (value >= 0x80)
                    {
                        index++;
                        count--;
                    }

                    result++;
                }
            }

            return result;
        }

        public int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int result = 0;

            while (byteCount > 0)
            {
                FFXIIITextTag tag = FFXIIITextTag.TryRead(bytes, ref byteIndex, ref byteCount);
                if (tag != null)
                {
                    result += tag.Write(chars, ref charIndex);
                }
                else
                {
                    int value = bytes[byteIndex++];
                    byteCount--;
                    if (value >= 0x80)
                    {
                        value = ValueToIndex(value, bytes[byteIndex++]);
                        byteCount--;
                    }
                    chars[charIndex++] = _codepage[(short)value];
                    result++;

                }
            }

            return result;
        }

        public static int ValueToIndex(int hight, int low)
        {
            switch (hight)
            {
                case 0x81:
                    if (low >= 0x80) low--;
                    return 256 + low - 0x40;
                case 0x85:
                    return low < 0x9E ? low + 0x40 : low + 0x21;
                    //return low < 0x80 ? low + 0x40 : low + 0x21;
            }

            throw new NotSupportedException(String.Format("{0}, {1}", hight, low));
        }

        public static int ValueToIndex(int value)
        {
            if (value <= 0xFF)
                return value;

            return ValueToIndex(value >> 8, value & 0xFF);
        }
    }
}