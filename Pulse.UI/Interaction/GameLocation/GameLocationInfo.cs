using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationInfo
    {
        public readonly string RootDirectory;
        public readonly string SystemDirectory;
        public readonly string MovieDirectory;
        public readonly string UpdatesDirectory;
        public readonly string AreasDirectory;

        private const string Part1ResourceDirName = "white_data";
        private const string Part2ResourceDirName = "alba_data";
        private const string ExecutableRelativePath = @"prog\win\bin\ffxiiiimg.exe";

        public GameLocationInfo(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            string resourcePath = Path.Combine(RootDirectory, ResourceDirName);
            SystemDirectory = Path.Combine(resourcePath, "sys");
            MovieDirectory = Path.Combine(resourcePath, "movie");
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

        public string ExecutablePath
        {
            get { return Path.Combine(RootDirectory, ResourceDirName, ExecutableRelativePath); }
        }

        public void Validate()
        {
            Exceptions.CheckDirectoryNotFoundException(SystemDirectory);
            Exceptions.CheckDirectoryNotFoundException(MovieDirectory);
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

        private readonly object _taskLock = new object();
        private UiArchives _archives;
        private Task<UiArchives> _archivesBuilderTask;

        public Task<UiArchives> ArchivesTree
        {
            get
            {
                lock (_taskLock)
                {
                    if (_archives != null)
                        return Task.Run(() => _archives);
                    if (_archivesBuilderTask != null)
                        return _archivesBuilderTask;

                    lock (_taskLock)
                        return _archivesBuilderTask = Task.Run(() =>
                        {
                            UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(this);
                            lock (_taskLock)
                                _archives = builder.Build();
                            return _archives;
                        });
                }
            }
        }

        public IEnumerable<String> EnumerateListingFiless()
        {
            if (Directory.Exists(SystemDirectory))
                foreach (String path in Directory.EnumerateFiles(SystemDirectory, "filelist*.bin"))
                    yield return path;

            if (Directory.Exists(UpdatesDirectory))
                foreach (String path in Directory.EnumerateFiles(UpdatesDirectory, "filelist*.bin"))
                    yield return path;
        }
    }
}