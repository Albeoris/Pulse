using System.Text;
using System.Xml;

namespace Pulse.Core
{
    public sealed class FFXIIITextEncoding : Encoding
    {
        private readonly Encoding _encoding;
        private readonly FFXIIICodePage _codepage;

        private readonly FFXIIITextEncoder _encoder;
        private readonly FFXIIITextDecoder _decoder;

        public FFXIIITextEncoding(Encoding encoding)
        {
            _encoding = encoding;
            _encoder = new FFXIIITextEncoder(encoding);
            _decoder = new FFXIIITextDecoder(encoding);
        }

        public FFXIIITextEncoding(FFXIIICodePage codepage)
        {
            _codepage = codepage;
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

        public void ToXml(XmlElement node)
        {
            if (_encoding != null)
            {
                node.SetInt32("CodePage", _encoding.CodePage);
                return;
            }

            XmlElement child = node.CreateChildElement("CodePage");
            FFXIIICodePageHelper.ToXml(_codepage, child);
        }

        public static FFXIIITextEncoding FromXml(XmlElement node)
        {
            int? codePage = node.FindInt32("CodePage");
            if (codePage != null)
            {
                Encoding encoding = GetEncoding(codePage.Value);
                return new FFXIIITextEncoding(encoding);
            }

            XmlElement child = node.GetChildElement("CodePage");
            FFXIIICodePage customCodePage = FFXIIICodePageHelper.FromXml(child);
            return new FFXIIITextEncoding(customCodePage);
        }
    }
}