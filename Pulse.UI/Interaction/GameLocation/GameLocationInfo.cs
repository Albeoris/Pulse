using System;
using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationInfo
    {
        public readonly string RootDirectory;
        public readonly string SystemDirectory;
        public readonly string UpdatesDirectory;
        public readonly string AreasDirectory;

        private const string Part1ResourceDirName = "white_data";
        private const string Part2ResourceDirName = "alba_data";

        public GameLocationInfo(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            string resourcePath = Path.Combine(RootDirectory, ResourceDirName);
            SystemDirectory = Path.Combine(resourcePath, "sys");
            AreasDirectory = Path.Combine(resourcePath, "zone");
            UpdatesDirectory = Path.Combine(resourcePath, "udp");
        }

        public static string ResourceDirName
        {
            get
            {
                switch (InteractionService.GamePart)
                {
                    case FFXIIIGamePart.Part1:
                        return Part1ResourceDirName;
                    case FFXIIIGamePart.Part2:
                        return Part2ResourceDirName;
                    default:
                        throw new NotImplementedException();
                }
            }
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