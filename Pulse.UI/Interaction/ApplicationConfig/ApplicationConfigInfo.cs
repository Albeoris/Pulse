using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigInfo
    {
        private static readonly object FileLock = new object();
        private static readonly object ScheduleLock = new object();

        public const string ConfigurationDirectory = ".\\Configuration";
        public const string ConfigurationFile = "Pulse.cfg";
        public const string LayoutConfigurationFile = "Layout.cfg";

        public GameLocationInfo GameLocation;
        public WorkingLocationInfo WorkingLocation;
        public String FileCommanderSelectedNodePath;
        public LocalizatorEnvironmentInfo LocalizatorEnvironment;

        public void Load()
        {
            lock (FileLock)
            {
                XmlElement config = XmlHelper.LoadDocument(ConfigurationFilePath);
                GameLocation = GameLocationInfo.FromXml(config["GameLocation"]);
                WorkingLocation = WorkingLocationInfo.FromXml(config["WorkingLocation"]);
                FileCommanderSelectedNodePath = config.FindString("FileCommanderSelectedNodePath");
                LocalizatorEnvironment = LocalizatorEnvironmentInfo.FromXml(config["LocalizatorEnvironment"]);
            }
        }

        public static string ConfigurationFilePath
        {
            get { return Path.Combine(ConfigurationDirectory, InteractionService.GamePart + "_" + ConfigurationFile); }
        }

        public static string LayoutConfigurationFilePath
        {
            get { return Path.Combine(ConfigurationDirectory, InteractionService.GamePart + "_" + LayoutConfigurationFile); }
        }

        public void Save()
        {
            lock (FileLock)
            {
                Directory.CreateDirectory(ConfigurationDirectory);
                XmlElement config = XmlHelper.CreateDocument("Configuration");

                GameLocation?.ToXml(config.CreateChildElement("GameLocation"));
                WorkingLocation?.ToXml(config.CreateChildElement("WorkingLocation"));
                if (FileCommanderSelectedNodePath != null) config.SetString("FileCommanderSelectedNodePath", FileCommanderSelectedNodePath);
                LocalizatorEnvironment?.ToXml(config.CreateChildElement("LocalizatorEnvironment"));

                config.GetOwnerDocument().Save(ConfigurationFilePath);
            }
        }

        public void ScheduleSave()
        {
            Task.Run(() =>
            {
                if (!Monitor.TryEnter(ScheduleLock, 0))
                    return;

                try
                {
                    Thread.Sleep(5000);
                    Save();
                }
                finally
                {
                    Monitor.Exit(ScheduleLock);
                }
            });
        }
    }
}