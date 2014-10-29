using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationInfo
    {
        public readonly string RootDirectory;

        public WorkingLocationInfo(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public void Validate()
        {
            Exceptions.CheckDirectoryNotFoundException(RootDirectory);
        }

        public void ToXml(XmlElement xmlElement)
        {
            xmlElement.SetString("RootDirectory", RootDirectory);
        }
        
        public static WorkingLocationInfo FromXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
                return null;

            string rootDirectory = xmlElement.FindString("RootDirectory");
            return new WorkingLocationInfo(rootDirectory);
        }
    }
}