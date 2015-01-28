using System;
using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextEncoder
    {
        private readonly FFXIIICodePage _codepage;

        public FFXIIITextEncoder(FFXIIICodePage codepage)
        {
            _codepage = Exceptions.CheckArgumentNull(codepage, "codepage");
        }

        public int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public int GetByteCount(char[] chars, int index, int count)
        {
            int result = 0;

            byte[] buff = new byte[2];
            while (count > 0)
            {
                FFXIIITextTag tag = FFXIIITextTag.TryRead(chars, ref index, ref count);
                if (tag != null)
                {
                    int offset = 0;
                    result += tag.Write(buff, ref offset);
                }
                else
                {
                    short value = _codepage[chars[index++]];

                    int hight, low;
                    IndexToValue(value, out hight, out low);

                    if (hight != 0)
                        result++;

                    result++;
                    count--;
                }
            }

            return result;
        }

        public int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int result = 0;

            while (charCount > 0)
            {
                FFXIIITextTag tag = FFXIIITextTag.TryRead(chars, ref charIndex, ref charCount);
                if (tag != null)
                {
                    result += tag.Write(bytes, ref byteIndex);
                }
                else
                {
                    short value = _codepage[chars[charIndex++]];

                    int hight, low;
                    IndexToValue(value, out hight, out low);

                    if (hight != 0)
                    {
                        bytes[byteIndex++] = (byte)hight;
                        result++;
                    }

                    bytes[byteIndex++] = (byte)low;
                    charCount--;
                    result++;
                }
            }

            return result;
        }

        public static void IndexToValue(int value, out int hight, out int low)
        {
            if (value <= 0xFF)
            {
                if (value < 0x80)
                {
                    hight = 0;
                    low = value;
                    return;
                }

                hight = 0x85;
                if (value >= 0xDE)
                {
                    low = value - 0x21;
                    return;
                }

                if (value >= 0x40)
                {
                    low = value - 0x40;
                    return;
                }

                throw new NotImplementedException();
            }
            
            value = 0x8180 + value - 0xFF;
            hight = (value & 0xFF00) >> 8;
            low = value & 0x00FF;
        }
    }
}