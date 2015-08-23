using System;
using System.Collections.Generic;
using System.IO;

namespace Pulse.UI
{
    public sealed class FileSystemInjectionSource : IUiInjectionSource
    {
        private Dictionary<string, string> _strings;

        public string ProvideRootDirectory()
        {
            return InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();
        }

        public bool DirectoryIsExists(string directoryPath)
        {
            return String.IsNullOrEmpty(directoryPath) || Directory.Exists(directoryPath);
        }

        public Stream TryOpen(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                return null;

            return File.OpenRead(sourcePath);
        }

        public void RegisterStrings(Dictionary<string, string> strings)
        {
            if (_strings != null)
                throw new NotSupportedException();

            _strings = strings;
        }

        public Dictionary<string,string>  TryProvideStrings()
        {
            return _strings;
        }
    }
}