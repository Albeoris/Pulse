using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.FS;

namespace Pulse.UI
{
    public class MemoryInjectionSource : IUiInjectionSource, IDisposable
    {
        private readonly Dictionary<string, Stream> _streams = new Dictionary<string, Stream>();
        private ZtrFileEntry[] _strings = null;

        public void RegisterStream(string sourcePath, Stream stream)
        {
            _streams.Add(sourcePath, stream);
        }

        public string ProvideRootDirectory()
        {
            return string.Empty;
        }

        public bool DirectoryIsExists(string directoryPath)
        {
            return true;
        }

        public Stream TryOpen(string sourcePath)
        {
            Stream result;
            if (_streams.TryGetValue(sourcePath, out result))
            {
                result.Position = 0;
                return result;
            }
            return null;
        }

        public void RegisterStrings(Dictionary<string, string> strings)
        {
            if (_strings != null)
                throw new NotSupportedException();

            _strings = strings.Select(pair => new ZtrFileEntry {Key = pair.Key, Value = pair.Value}).ToArray();
        }

        public ZtrFileEntry[] TryProvideStrings()
        {
            return _strings;
        }

        public void Dispose()
        {
            foreach (Stream stream in _streams.Values)
                stream.Dispose();
        }
    }
}