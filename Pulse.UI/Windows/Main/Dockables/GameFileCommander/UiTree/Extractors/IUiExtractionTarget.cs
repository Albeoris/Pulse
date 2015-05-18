using System;
using System.IO;
using Pulse.Core;

namespace Pulse.UI
{
    public interface IUiExtractionTarget
    {
        StreamSequence Create(String targetPath);
        void CreateDirectory(string directoryPath);
    }
}