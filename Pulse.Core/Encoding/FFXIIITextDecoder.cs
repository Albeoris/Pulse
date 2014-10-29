using System;
using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextDecoder
    {
        private readonly Encoding _encoding;
        private readonly FFXIIICodePage _codepage;

        public FFXIIITextDecoder(Encoding encoding)
        {
            _encoding = Exceptions.CheckArgumentNull(encoding, "encoding");
        }

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
                    if (_encoding != null)
                    {
                        int charsWrited = 0;
                        int bytesReaded = 1;
                        for (; bytesReaded < 5; bytesReaded++)
                        {
                            charsWrited = _encoding.GetCharCount(bytes, index, bytesReaded);
                            if (charsWrited > 0)
                                break;
                        }
                        count -= bytesReaded;
                        index += bytesReaded;
                        result += charsWrited;
                    }
                    else
                    {
                        count--;
                        index++;
                        result++;
                    }
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
                    //if (bytes[byteIndex] != 0x00 && bytes[byteIndex] != 0x09 && (bytes[byteIndex] < 0x20 || bytes[byteIndex] >'z'))
                    //    throw new Exception();

                    if (_encoding != null)
                    {
                        int charsWrited = 0;
                        int bytesReaded = 1;
                        for (; bytesReaded < 5; bytesReaded++)
                        {
                            charsWrited = _encoding.GetChars(bytes, byteIndex, bytesReaded, chars, charIndex);
                            if (charsWrited > 0)
                                break;
                        }
                        charIndex += charsWrited;
                        byteIndex += bytesReaded;
                        byteCount -= bytesReaded;
                        result += charsWrited;
                    }
                    else
                    {
                        chars[charIndex++] = _codepage[bytes[byteIndex++]];
                        byteCount--;
                        result++;
                    }
                }
            }

            return result;
        }
    }
}