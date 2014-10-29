using System.Text;

namespace Pulse.Core
{
    public static class FFXIIITextEncodingFactory
    {
        public static FFXIIITextEncoding CreateEuro()
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            return new FFXIIITextEncoding(encoding);
        }

        public static FFXIIITextEncoding CreateCyrillic()
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.CreateCyrillic();
            return new FFXIIITextEncoding(codepage);
        }

        public static FFXIIITextEncoding CreateJapanese()
        {
            Encoding encoding = Encoding.GetEncoding(932);
            return new FFXIIITextEncoding(encoding);
        }
    }
}