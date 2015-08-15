using System;
using System.Globalization;
using System.Xml;

namespace Pulse.Core
{
    public static class XmlElementExm
    {
        public static XmlDocument GetOwnerDocument(this XmlElement self)
        {
            Exceptions.CheckArgumentNull(self, "self");

            XmlDocument doc = self.OwnerDocument;
            if (doc == null)
                throw new ArgumentException("XmlDocument не найден.", nameof(self));

            return doc;
        }

        public static XmlElement GetChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlElement child = self[name];
            if (child == null)
                throw Exceptions.CreateArgumentException("self", "Дочерний XmlElement '{0}' не найден.", name);

            return child;
        }

        public static XmlElement GetChildElement(this XmlElement self, int index)
        {
            Exceptions.CheckArgumentNull(self, "self");

            XmlElement child = self.ChildNodes[index] as XmlElement;
            if (child == null)
                throw Exceptions.CreateArgumentException("self", "Дочерний XmlElement '{0}' не найден.", index);

            return child;
        }

        public static XmlAttribute GetChildAttribute(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.Attributes[name];
            if (attr == null)
                throw Exceptions.CreateArgumentException("self", "XmlAttribute '{0}' не найден.", name);
            
            return attr;
        }

        public static XmlElement CreateChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlDocument doc = self.GetOwnerDocument();
            XmlElement element = doc.CreateElement(name);
            self.AppendChild(element);

            return element;
        }

        public static XmlElement EnsureChildElement(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlElement element = self[name];
            if (element != null)
                return element;

            XmlDocument doc = self.GetOwnerDocument();
            element = doc.CreateElement(name);
            self.AppendChild(element);

            return element;
        }

        public static XmlAttribute EnsureChildAttribute(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.Attributes[name];
            if (attr != null)
                return attr;

            XmlDocument doc = self.GetOwnerDocument();
            attr = doc.CreateAttribute(name);
            self.Attributes.Append(attr);

            return attr;
        }

        public static Boolean? FindBoolean(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToBoolean(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Char? FindChar(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToChar(arg.Value, CultureInfo.InvariantCulture);
        }

        public static SByte? FindSByte(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToSByte(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Byte? FindByte(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToByte(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Int16? FindInt16(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt16(arg.Value, CultureInfo.InvariantCulture);
        }

        public static UInt16? FindUInt16(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToUInt16(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Int32? FindInt32(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt32(arg.Value, CultureInfo.InvariantCulture);
        }

        public static UInt32? FindUInt32(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;
            
            return Convert.ToUInt32(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Int64? FindInt64(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToInt64(arg.Value, CultureInfo.InvariantCulture);
        }

        public static UInt64? FindUInt64(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToUInt64(arg.Value, CultureInfo.InvariantCulture);
        }

        public static Double? FindDouble(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            if (arg == null)
                return null;

            return Convert.ToDouble(arg.Value, CultureInfo.InvariantCulture);
        }

        public static string FindString(this XmlElement self, string name)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute arg = self.Attributes[name];
            return arg?.Value;
        }

        public static Boolean GetBoolean(this XmlElement self, string name)
        {
            Boolean? value = FindBoolean(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Char GetChar(this XmlElement self, string name)
        {
            Char? value = FindChar(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static SByte GetSByte(this XmlElement self, string name)
        {
            SByte? value = FindSByte(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Byte GetByte(this XmlElement self, string name)
        {
            Byte? value = FindByte(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Int16 GetInt16(this XmlElement self, string name)
        {
            Int16? value = FindInt16(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static UInt16 GetUInt16(this XmlElement self, string name)
        {
            UInt16? value = FindUInt16(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Int32 GetInt32(this XmlElement self, string name)
        {
            Int32? value = FindInt32(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static UInt32 GetUInt32(this XmlElement self, string name)
        {
            UInt32? value = FindUInt32(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Int64 GetInt64(this XmlElement self, string name)
        {
            Int64? value = FindInt64(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static UInt64 GetUInt64(this XmlElement self, string name)
        {
            UInt64? value = FindUInt64(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static Double GetDouble(this XmlElement self, string name)
        {
            Double? value = FindDouble(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value.Value;
        }

        public static string GetString(this XmlElement self, string name)
        {
            string value = FindString(self, name);
            if (value == null)
                throw Exceptions.CreateException("Аттрибут '{0}' не найден.", name);

            return value;
        }

        public static void SetBoolean(this XmlElement self, string name, Boolean value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetChar(this XmlElement self, string name, Char value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetSByte(this XmlElement self, string name, SByte value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetByte(this XmlElement self, string name, Byte value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt16(this XmlElement self, string name, Int16 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt16(this XmlElement self, string name, UInt16 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt32(this XmlElement self, string name, Int32 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt32(this XmlElement self, string name, UInt32 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetInt64(this XmlElement self, string name, Int64 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetUInt64(this XmlElement self, string name, UInt64 value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetDouble(this XmlElement self, string name, Double value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static void SetString(this XmlElement self, string name, string value)
        {
            Exceptions.CheckArgumentNull(self, "self");
            Exceptions.CheckArgumentNullOrEmprty(name, "name");

            XmlAttribute attr = self.EnsureChildAttribute(name);
            attr.Value = value;
        }
    }
}