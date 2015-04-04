using System;
using System.IO;

namespace Pulse.UI
{
    public interface IUiExtractionTarget
    {
        Stream Create(String targetPath);
        void CreateDirectory(string directoryPath);
    }
}