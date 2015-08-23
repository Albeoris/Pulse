using System;
using System.IO;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class FileSystemExtractionTarget : IUiExtractionTarget
    {
        public void CreateDirectory(string directoryPath)
        {
            if (!String.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public StreamSequence Create(string targetPath)
        {
            String directoryPath = Path.GetDirectoryName(targetPath);
            if (!String.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            FileSequencedStreamFactory factory = new FileSequencedStreamFactory(targetPath, FileMode.Create, FileAccess.Write);
            return new StreamSequence(factory);
        }
    }
}