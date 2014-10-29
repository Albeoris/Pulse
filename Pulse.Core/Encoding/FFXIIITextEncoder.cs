using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextEncoder
    {
        private readonly Encoding _encoding;
        private readonly FFXIIICodePage _codepage;

        public FFXIIITextEncoder(Encoding encoding)
        {
            _encoding = Exceptions.CheckArgumentNull(encoding, "encoding");
        }

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
                    if (_encoding != null)
                    {
                        result += _encoding.GetByteCount(chars, index, 1);
                        count--;
                        index++;
                    }
                    else
                    {
                        result++;
                        index++;
                        count--;
                    }
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
                    if (_encoding != null)
                    {
                        int bytesWrited = _encoding.GetBytes(chars, charIndex, 1, bytes, byteIndex);
                        byteIndex += bytesWrited;
                        result += bytesWrited;
                        charCount--;
                        charIndex++;
                    }
                    else
                    {
                        bytes[byteIndex++] = _codepage[chars[charIndex++]];
                        charCount--;
                        result++;
                    }
                }
            }

            return result;
        }
    }
}