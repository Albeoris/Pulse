using System;
using System.Collections.Generic;
using System.IO;

namespace Pulse.UI
{
    public sealed class MemoryInjectionAccessor : MemoryInjectionSource, IUiExtractionTarget, IDisposable
    {
        public Stream Create(string targetPath)
        {
            MemoryStream result = new MemoryStream(4096);
            RegisterStream(targetPath, result);
            return result;
        }

        public void CreateDirectory(string directoryPath)
        {
        }
    }
}