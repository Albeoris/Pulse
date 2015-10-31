using System;
using System.Xml;

namespace Pulse.Core
{
    public static class XmlDocumentExm
    {
        public static XmlElement GetDocumentElement(this XmlDocument self)
        {
            Exceptions.CheckArgumentNull(self, "self");

            XmlElement element = self.DocumentElement;
            if (element == null)
                throw new ArgumentException("XmlElement was not found.", nameof(self));

            return element;
        }
    }
}