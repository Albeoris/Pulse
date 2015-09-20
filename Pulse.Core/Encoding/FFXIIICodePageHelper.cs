using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pulse.Core
{
    public static class FFXIIICodePageHelper
    {
        public static FFXIIICodePage CreateEuro()
        {
            return Create(Encoding.GetEncoding(1252));
        }

        public static FFXIIICodePage CreateCyrillic()
        {
            return Create(Encoding.GetEncoding(1251));
        }

        public static unsafe FFXIIICodePage Create(Encoding encoding)
        {
            byte[] buff = new byte[256];
            char[] chars = new char[256 + 0x2C10];

            fixed (byte* buffPtr = &buff[0])
            fixed (char* charsPtr = &chars[0])
            {
                for (int b = 0; b < 256; b++)
                    buffPtr[b] = (byte)b;

                encoding.GetChars(buffPtr, buff.Length, charsPtr, buff.Length);
            }

            CreateAdditionalCharacters(chars);
            
            Dictionary<char, short> bytes = new Dictionary<char, short>(chars.Length);
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                char ch = chars[i];
                switch (ch)
                {
                    case '\0':
                        continue;
                }
                bytes[chars[i]] = (short)i;
            }

            return new FFXIIICodePage(chars, bytes);
        }

        private static void CreateAdditionalCharacters(char[] chars)
        {
            //chars[256] = 's'; // Spanish

            chars[FFXIIIEncodingMap.ValueToIndex(0x8141)] = '､';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8142)] = '｡';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8145)] = '･';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8146)] = '︓';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8148)] = '︖';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8149)] = '︕';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8151)] = '＿';
            chars[FFXIIIEncodingMap.ValueToIndex(0x815B)] = '—';
            chars[FFXIIIEncodingMap.ValueToIndex(0x815C)] = '―';
            chars[FFXIIIEncodingMap.ValueToIndex(0x815E)] = '／';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8160)] = '〜';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8163)] = '…';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8169)] = '（';
            chars[FFXIIIEncodingMap.ValueToIndex(0x816A)] = '）';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8173)] = '《';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8174)] = '》';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8175)] = '「';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8176)] = '」';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8179)] = '【';
            chars[FFXIIIEncodingMap.ValueToIndex(0x817A)] = '】';
            chars[FFXIIIEncodingMap.ValueToIndex(0x817B)] = '＋';
            chars[FFXIIIEncodingMap.ValueToIndex(0x817C)] = '－';
            chars[FFXIIIEncodingMap.ValueToIndex(0x817E)] = '✕';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8181)] = '＝';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8183)] = '＜';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8184)] = '＞';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8187)] = '∞';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8193)] = '％';
            chars[FFXIIIEncodingMap.ValueToIndex(0x8195)] = '＆';
            chars[FFXIIIEncodingMap.ValueToIndex(0x819A)] = '★';
            chars[FFXIIIEncodingMap.ValueToIndex(0x819B)] = '◯';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81A0)] = '⬜';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81A2)] = '△';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81A6)] = '⁜';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81A8)] = '→';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81A9)] = '←';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81AA)] = '↑';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81AB)] = '↓';
            chars[FFXIIIEncodingMap.ValueToIndex(0x81F4)] = '♬';
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

            foreach (KeyValuePair<char, short> pair in codepage.Codes)
            {
                XmlElement byteNode = bytesNode.CreateChildElement("Entry");
                byteNode.SetChar("Char", pair.Key);
                byteNode.SetInt16("Byte", pair.Value);
            }
        }

        public static FFXIIICodePage FromXml(XmlElement node)
        {
            XmlElement charsNode = node.GetChildElement("Chars");
            XmlElement bytesNode = node.GetChildElement("Bytes");
            if (charsNode.ChildNodes.Count != 11536) throw Exceptions.CreateException("Неверное число дочерних элементов узла '{0}': {1}. Ожидается: 11536", charsNode.Name, charsNode.ChildNodes.Count);

            char[] chars = new char[256 + 0x2C10];
            Dictionary<char, short> bytes = new Dictionary<char, short>(256);

            for (int i = 0; i < chars.Length; i++)
            {
                XmlElement charNode = (XmlElement)charsNode.ChildNodes[i];
                chars[i] = charNode.GetChar("Char");
            }

            foreach (XmlElement byteNode in bytesNode)
                bytes[byteNode.GetChar("Char")] = byteNode.GetInt16("Byte");

            return new FFXIIICodePage(chars, bytes);
        }
    }
}
