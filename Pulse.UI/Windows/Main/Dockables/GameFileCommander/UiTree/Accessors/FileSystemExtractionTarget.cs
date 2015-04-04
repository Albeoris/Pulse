using System;
using System.IO;

namespace Pulse.UI
{
    public sealed class FileSystemExtractionTarget : IUiExtractionTarget
    {
        public void CreateDirectory(string directoryPath)
        {
            if (!String.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public Stream Create(string targetPath)
        {
            String directoryPath = Path.GetDirectoryName(targetPath);
            if (!String.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return File.Create(targetPath);
        }
    }
}