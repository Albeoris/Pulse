using System.Collections.Generic;
using System.IO;
using System.Xml;
using Pulse.Core;
using Dic = System.Collections.Generic.Dictionary<string, Pulse.Core.FFXIIITextEncoding>;

namespace Pulse.UI
{
    public sealed class TextEncodingInfo
    {
        private readonly Dic _dic = new Dic();

        public void Add(string loc, FFXIIITextEncoding encoding)
        {
            _dic.Add(loc.ToLower(), encoding);
        }

        public FFXIIITextEncoding this[string loc]
        {
            get
            {
                FFXIIITextEncoding encoding;
                if (!_dic.TryGetValue(loc.ToLower(), out encoding))
                    throw Exceptions.CreateException("Отсутствует данные о кодировкие [Локаль: {0}].");
                return encoding;
            }
        }

        public void ToXml(XmlElement node)
        {
            foreach (KeyValuePair<string, FFXIIITextEncoding> pair in _dic)
            {
                XmlElement child = node.CreateChildElement(pair.Key);
                pair.Value.ToXml(child);
            }
        }

        public static TextEncodingInfo FromXml(XmlElement node)
        {
            if (node == null)
                return null;

            TextEncodingInfo result = new TextEncodingInfo();
            foreach (XmlElement child in node)
                result.Add(child.Name, FFXIIITextEncoding.FromXml(child));

            return result;
        }

        public static TextEncodingInfo CreateDefault()
        {
            TextEncodingInfo result = new TextEncodingInfo();
            result.Add("ru", FFXIIITextEncodingFactory.CreateCyrillic());

            FFXIIITextEncoding euro = FFXIIITextEncodingFactory.CreateEuro();
            result.Add("de", euro);
            result.Add("fr", euro);
            result.Add("it", euro);
            result.Add("us", euro);

            return result;
        }

        public static TextEncodingInfo Load()
        {
            string filePath = Path.Combine(InteractionService.WorkingLocation.Provide().RootDirectory, "TextEncoding.xml");
            XmlElement doc = XmlHelper.LoadDocument(filePath);
            return FromXml(doc);
        }

        public void Save()
        {
            string filePath = Path.Combine(InteractionService.WorkingLocation.Provide().RootDirectory, "TextEncoding.xml");
            XmlElement doc = XmlHelper.CreateDocument("TextEncoding");
            ToXml(doc);
            doc.GetOwnerDocument().Save(filePath);
        }
    }
}