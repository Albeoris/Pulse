using System;
using System.Text;

namespace Pulse.Text
{
    public sealed class FFXIIITextEncoder
    {
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
                    result += Encoding.UTF8.GetByteCount(chars, index, 1);
                    count--;
                    index++;
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
                    int bytesWrited = Encoding.UTF8.GetBytes(chars, charIndex, 1, bytes, byteIndex);
                    byteIndex += bytesWrited;
                    result += bytesWrited;
                    charCount--;
                    charIndex++;
                }
            }

            return result;
        }
    }
}