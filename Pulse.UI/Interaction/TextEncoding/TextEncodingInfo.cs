using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingInfo
    {
        public readonly FFXIIITextEncoding Encoding;

        public TextEncodingInfo(FFXIIITextEncoding encoding)
        {
            Encoding = Exceptions.CheckArgumentNull(encoding, "encoding");
        }

        public static TextEncodingInfo CreateDefault()
        {
            return new TextEncodingInfo(FFXIIITextEncodingFactory.CreateEuro());
        }

        public static TextEncodingInfo Load()
        {
            string filePath = Path.Combine(InteractionService.WorkingLocation.Provide().RootDirectory, "TextEncoding.xml");

            XmlElement codepageXml = XmlHelper.LoadDocument(filePath);
            return FromXml(codepageXml);
        }

        public void Save()
        {
            string filePath = Path.Combine(InteractionService.WorkingLocation.Provide().RootDirectory, "TextEncoding.xml");
            XmlElement doc = XmlHelper.CreateDocument("TextEncoding");
            ToXml(doc);
            doc.GetOwnerDocument().Save(filePath);
        }

        private static TextEncodingInfo FromXml(XmlElement codepageNode)
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.FromXml(codepageNode);
            FFXIIITextEncoding encoding = new FFXIIITextEncoding(codepage);

            return new TextEncodingInfo(encoding);
        }

        private void ToXml(XmlElement codepageNode)
        {
            FFXIIICodePageHelper.ToXml(Encoding.Codepage, codepageNode);
        }
    }
}