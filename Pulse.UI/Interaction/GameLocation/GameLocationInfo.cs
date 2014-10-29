using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationInfo
    {
        public readonly string RootDirectory;
        public readonly string SystemDirectory;
        public readonly string AreasDirectory;

        public GameLocationInfo(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            SystemDirectory = Path.Combine(RootDirectory, "white_data", "sys");
            AreasDirectory = Path.Combine(RootDirectory, "white_data", "zone");
        }

        public void Validate()
        {
            Exceptions.CheckDirectoryNotFoundException(SystemDirectory);
            Exceptions.CheckDirectoryNotFoundException(AreasDirectory);
        }

        public void ToXml(XmlElement xmlElement)
        {
            xmlElement.SetString("RootDirectory", RootDirectory);
        }
        
        public static GameLocationInfo FromXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
                return null;

            string rootDirectory = xmlElement.FindString("RootDirectory");
            return new GameLocationInfo(rootDirectory);
        }
    }
}