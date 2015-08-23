using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.UI
{
    public class MemoryInjectionSource : IUiInjectionSource, IDisposable
    {
        private readonly Dictionary<string, Stream> _streams = new Dictionary<string, Stream>();
        private Dictionary<string,string> _strings;

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
                return new StreamSegment(result, 0, result.Length, FileAccess.Read);
            if (_streams.Count == 1 && _streams.TryGetValue(String.Empty, out result))
                return new StreamSegment(result, 0, result.Length, FileAccess.Read);
            return null;
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

        public void Dispose()
        {
            foreach (Stream stream in _streams.Values)
                stream.Dispose();
        }
    }
}