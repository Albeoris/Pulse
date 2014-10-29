using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pulse.Core
{
    public static class FFXIIICodePageHelper
    {
        public static FFXIIICodePage Create(string[] values)
        {
            char[] chars = new char[224];
            Dictionary<char, byte> bytes = new Dictionary<char, byte>(224);

            for (byte i = 0; i < 224; i++)
            {
                string value = values[i];
                chars[i] = string.IsNullOrEmpty(value) ? '\0' : value[0];
                foreach (var ch in value)
                    bytes[ch] = i;
            }

            return new FFXIIICodePage(chars, bytes);
        }

        public static unsafe FFXIIICodePage CreateCyrillic()
        {
            const string cyrillic = "ÁÃÄ¨ÆÇÈÉËÏÓÔÖ×ØÙÚÛÜÝÞßáâãäæçéêëìíïòôö÷øùúûüýþÿ¸";
            Encoding ansi = Encoding.GetEncoding(1252);

            byte[] buff = new byte[224];
            char[] chars = new char[224];

            fixed (char* cyrillicPtr = cyrillic)
            fixed (byte* buffPtr = &buff[0])
            fixed (char* charsPtr = &chars[0])
            {
                for (byte b = 0x20; b < 0x7B; b++)
                    buffPtr[b - 0x20] = b;

                ansi.GetChars(buffPtr, 0x7A, charsPtr, 0x7A);

                for (int i = 0; i < cyrillic.Length; i++)
                    charsPtr[0x7B + i - 0x20] = cyrillicPtr[i];
            }

            Dictionary<char, byte> bytes = new Dictionary<char, byte>(chars.Length);
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                char ch = chars[i];
                switch (ch)
                {
                    case '\0':
                        continue;
                }
                bytes[chars[i]] = (byte)i;
            }

            bytes['À'] = bytes['A'];
            bytes['Â'] = bytes['B'];
            bytes['Å'] = bytes['E'];
            bytes['Ê'] = bytes['K'];
            bytes['Ì'] = bytes['M'];
            bytes['Í'] = bytes['H'];
            bytes['Î'] = bytes['O'];
            bytes['Ð'] = bytes['P'];
            bytes['Ñ'] = bytes['C'];
            bytes['Ò'] = bytes['T'];
            bytes['Õ'] = bytes['X'];
            bytes['à'] = bytes['a'];
            bytes['å'] = bytes['e'];
            bytes['è'] = bytes['u'];
            bytes['î'] = bytes['o'];
            bytes['ð'] = bytes['p'];
            bytes['ñ'] = bytes['c'];
            bytes['ó'] = bytes['y'];
            bytes['õ'] = bytes['x'];

            return new FFXIIICodePage(chars, bytes);
        }

        public static void ToXml(FFXIIICodePage codepage, XmlElement node)
        {
            XmlElement charsNode = node.EnsureChildElement("Chars");
            XmlElement bytesNode = node.EnsureChildElement("Bytes");

            foreach (char ch in codepage.Chars)
            {
                XmlElement charNode = charsNode.CreateChildElement("Entry");
                charNode.SetChar("Char", ch);
            }

            foreach (KeyValuePair<char, byte> pair in codepage.Bytes)
            {
                XmlElement byteNode = bytesNode.CreateChildElement("Entry");
                byteNode.SetChar("Char", pair.Key);
                byteNode.SetByte("Byte", pair.Value);
            }
        }

        public static FFXIIICodePage FromXml(XmlElement node)
        {
            XmlElement charsNode = node.GetChildElement("Chars");
            XmlElement bytesNode = node.GetChildElement("Bytes");
            if (charsNode.ChildNodes.Count != 224) throw Exceptions.CreateException("Íåâåðíîå ÷èñëî äî÷åðíèõ ýëåìåíòîâ óçëà '{0}': {1}. Îæèäàåòñÿ: 224", charsNode.Name, charsNode.ChildNodes.Count);

            char[] chars = new char[224];
            Dictionary<char, byte> bytes = new Dictionary<char, byte>(224);

            for (int i = 0; i < chars.Length; i++)
            {
                XmlElement charNode = (XmlElement)charsNode.ChildNodes[i];
                chars[i] = charNode.GetChar("Char");
            }

            foreach (XmlElement byteNode in bytesNode)
                bytes[byteNode.GetChar("Char")] = byteNode.GetByte("Byte");

            return new FFXIIICodePage(chars, bytes);
        }
    }
}