using System;
using System.Text;

namespace Pulse.Text
{
    public sealed class FFXIIITextDecoder
    {
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
                    int charsWrited = 0;
                    int bytesReaded = 1;
                    for (; bytesReaded < 5; bytesReaded++)
                    {
                        charsWrited = Encoding.UTF8.GetCharCount(bytes, index, bytesReaded);
                        if (charsWrited > 0)
                            break;
                    }
                    count -= bytesReaded;
                    index += bytesReaded;
                    result += charsWrited;
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
                    //if (bytes[byteIndex] != 0x00 && bytes[byteIndex] != 0x09 && (bytes[byteIndex] < 0x20 || bytes[byteIndex] > 0x8B))
                    //    throw new Exception();

                    int charsWrited = 0;
                    int bytesReaded = 1;
                    for (; bytesReaded < 5; bytesReaded++)
                    {
                        charsWrited = Encoding.UTF8.GetChars(bytes, byteIndex, bytesReaded, chars, charIndex);
                        if (charsWrited > 0)
                            break;
                    }
                    charIndex += charsWrited;
                    byteIndex += bytesReaded;
                    byteCount -= bytesReaded;
                    result += charsWrited;
                }
            }

            return result;
        }
    }
}