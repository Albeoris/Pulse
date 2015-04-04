using System;
using System.IO;

namespace Pulse.UI
{
    public sealed class FileSystemInjectionSource : IUiInjectionSource
    {
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
    }
}