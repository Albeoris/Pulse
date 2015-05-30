using System;

namespace Pulse.Core
{
    public static class FFXIIITextEncodingFactory
    {
        public static FFXIIITextEncoding CreateEuro()
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.CreateEuro();
            return new FFXIIITextEncoding(codepage);
        }

        public static FFXIIITextEncoding CreateCyrillic()
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.CreateCyrillic();
            return new FFXIIITextEncoding(codepage);
        }

        public static readonly Lazy<FFXIIITextEncoding> DefaultEuroEncoding = new Lazy<FFXIIITextEncoding>(CreateEuro);
    }
}