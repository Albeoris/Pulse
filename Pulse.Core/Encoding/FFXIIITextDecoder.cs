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
                    continue;
                }

                FFXIIITextReference reference = FFXIIITextReference.TryRead(bytes, ref index, ref count);
                if (reference != null)
                {
                    result += reference.SizeInChars;
                    continue;
                }

                byte value = bytes[index++];
                count--;

                if (value >= 0x80)
                {
                    index++;
                    count--;
                }

                result++;
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
                    continue;
                }

                FFXIIITextReference reference = FFXIIITextReference.TryRead(bytes, ref byteIndex, ref byteCount);
                if (reference != null)
                {
                    result += reference.Write(chars, ref charIndex);
                    continue;
                }

                int value = bytes[byteIndex++];
                byteCount--;
                if (value >= 0x80)
                {
                    value = FFXIIIEncodingMap.ValueToIndex(value, bytes[byteIndex++]);
                    byteCount--;
                }
                chars[charIndex++] = _codepage[(short)value];
                result++;
            }

            return result;
        }
    }
}