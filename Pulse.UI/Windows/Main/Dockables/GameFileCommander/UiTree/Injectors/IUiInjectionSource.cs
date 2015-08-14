using System;
using System.Collections.Generic;
using System.IO;

namespace Pulse.UI
{
    public interface IUiInjectionSource
    {
        String ProvideRootDirectory();
        Boolean DirectoryIsExists(string directoryPath);
        Stream TryOpen(String sourcePath);
        Dictionary<string, string> TryProvideStrings();
    }
}