using System.Text;
using System.Xml;

namespace Pulse.Core
{
    public sealed class FFXIIITextEncoding : Encoding
    {
        public readonly FFXIIICodePage Codepage;

        private readonly FFXIIITextEncoder _encoder;
        private readonly FFXIIITextDecoder _decoder;

        public FFXIIITextEncoding(FFXIIICodePage codepage)
        {
            Codepage = codepage;
            _encoder = new FFXIIITextEncoder(codepage);
            _decoder = new FFXIIITextDecoder(codepage);
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return _encoder.GetByteCount(chars, index, count);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return _encoder.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return _decoder.GetCharCount(bytes, index, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return _decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return _encoder.GetMaxByteCount(charCount);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return _decoder.GetMaxCharCount(byteCount);
        }
    }
}