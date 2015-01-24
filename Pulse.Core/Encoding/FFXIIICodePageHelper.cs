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
            chars[256] = 's'; // Spanish

            chars[FFXIIITextDecoder.ValueToIndex(0x8141)] = '､';
            chars[FFXIIITextDecoder.ValueToIndex(0x8142)] = '｡';
            chars[FFXIIITextDecoder.ValueToIndex(0x8145)] = '･';
            chars[FFXIIITextDecoder.ValueToIndex(0x8146)] = '︓';
            chars[FFXIIITextDecoder.ValueToIndex(0x8148)] = '︖';
            chars[FFXIIITextDecoder.ValueToIndex(0x8149)] = '︕';
            chars[FFXIIITextDecoder.ValueToIndex(0x8151)] = '＿';
            chars[FFXIIITextDecoder.ValueToIndex(0x815B)] = '—';
            chars[FFXIIITextDecoder.ValueToIndex(0x815C)] = '―';
            chars[FFXIIITextDecoder.ValueToIndex(0x815E)] = '／';
            chars[FFXIIITextDecoder.ValueToIndex(0x8160)] = '〜';
            chars[FFXIIITextDecoder.ValueToIndex(0x8163)] = '…';
            chars[FFXIIITextDecoder.ValueToIndex(0x8169)] = '（';
            chars[FFXIIITextDecoder.ValueToIndex(0x816A)] = '）';
            chars[FFXIIITextDecoder.ValueToIndex(0x8173)] = '《';
            chars[FFXIIITextDecoder.ValueToIndex(0x8174)] = '》';
            chars[FFXIIITextDecoder.ValueToIndex(0x8175)] = '「';
            chars[FFXIIITextDecoder.ValueToIndex(0x8176)] = '」';
            chars[FFXIIITextDecoder.ValueToIndex(0x8179)] = '【';
            chars[FFXIIITextDecoder.ValueToIndex(0x817A)] = '】';
            chars[FFXIIITextDecoder.ValueToIndex(0x817B)] = '＋';
            chars[FFXIIITextDecoder.ValueToIndex(0x817C)] = '－';
            chars[FFXIIITextDecoder.ValueToIndex(0x817E)] = '✕';
            chars[FFXIIITextDecoder.ValueToIndex(0x8181)] = '＝';
            chars[FFXIIITextDecoder.ValueToIndex(0x8183)] = '＜';
            chars[FFXIIITextDecoder.ValueToIndex(0x8184)] = '＞';
            chars[FFXIIITextDecoder.ValueToIndex(0x8193)] = '％';
            chars[FFXIIITextDecoder.ValueToIndex(0x8195)] = '＆';
            chars[FFXIIITextDecoder.ValueToIndex(0x819A)] = '★';
            chars[FFXIIITextDecoder.ValueToIndex(0x819B)] = '◯';
            chars[FFXIIITextDecoder.ValueToIndex(0x81A0)] = '⬜';
            chars[FFXIIITextDecoder.ValueToIndex(0x81A2)] = '△';
            chars[FFXIIITextDecoder.ValueToIndex(0x81A6)] = '⁜';
            chars[FFXIIITextDecoder.ValueToIndex(0x81A8)] = '→';
            chars[FFXIIITextDecoder.ValueToIndex(0x81A9)] = '←';
            chars[FFXIIITextDecoder.ValueToIndex(0x81AA)] = '↑';
            chars[FFXIIITextDecoder.ValueToIndex(0x81AB)] = '↓';
            chars[FFXIIITextDecoder.ValueToIndex(0x81F4)] = '♬';
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