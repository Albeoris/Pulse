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
                    continue;
                }

                FFXIIITextReference reference = FFXIIITextReference.TryRead(chars, ref index, ref count);
                if (reference != null)
                {
                    result += reference.SizeInBytes;
                    continue;
                }

                short value = _codepage[chars[index++]];

                int hight, low;
                FFXIIIEncodingMap.IndexToValue(value, out hight, out low);

                if (hight != 0)
                    result++;

                result++;
                count--;
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
                    continue;
                }

                FFXIIITextReference reference = FFXIIITextReference.TryRead(chars, ref charIndex, ref charCount);
                if (reference != null)
                {
                    result += reference.Write(bytes, ref byteIndex);
                    continue;
                }

                short value = _codepage[chars[charIndex++]];

                int hight, low;
                FFXIIIEncodingMap.IndexToValue(value, out hight, out low);

                if (hight != 0)
                {
                    bytes[byteIndex++] = (byte)hight;
                    result++;
                }

                bytes[byteIndex++] = (byte)low;
                charCount--;
                result++;
            }

            return result;
        }
    }
}