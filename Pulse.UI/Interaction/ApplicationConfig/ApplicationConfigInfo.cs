using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigInfo
    {
        public const string ConfigurationDirectory = ".\\Configuration";
        public const string ConfigurationFile = "Pulse.cfg";
        public const string LayoutConfigurationFile = "Layout.cfg";
        public static readonly string ConfigurationFilePath = Path.Combine(ConfigurationDirectory, ConfigurationFile);
        public static readonly string LayoutConfigurationFilePath = Path.Combine(ConfigurationDirectory, LayoutConfigurationFile);

        public GameLocationInfo GameLocation;
        public WorkingLocationInfo WorkingLocation;

        public void Load()
        {
            XmlElement config = XmlHelper.LoadDocument(ConfigurationFilePath);
            GameLocation = GameLocationInfo.FromXml(config["GameLocation"]);
            WorkingLocation = WorkingLocationInfo.FromXml(config["WorkingLocation"]);
        }

        public void Save()
        {
            Directory.CreateDirectory(ConfigurationDirectory);
            XmlElement config = XmlHelper.CreateDocument("Configuration");
            
            if (GameLocation != null) GameLocation.ToXml(config.CreateChildElement("GameLocation"));
            if (WorkingLocation != null) WorkingLocation.ToXml(config.CreateChildElement("WorkingLocation"));

            config.GetOwnerDocument().Save(ConfigurationFilePath);
        }
    }
}