using System;
using System.IO;

namespace Pulse.Core
{
    public sealed class FileSequencedStreamFactory : ISequencedStreamFactory
    {
        private string _extension;
        private string _filePath;
        private FileMode _mode;
        private FileAccess _access;

        public FileSequencedStreamFactory(String filePath, FileMode mode, FileAccess access)
        {
            _extension = Path.GetExtension(filePath);
            _filePath = filePath;
            if (!string.IsNullOrEmpty(_extension))
                _filePath = filePath.Substring(0, _filePath.Length - _extension.Length);

            _mode = mode;
            _access = access;
        }

        public bool TryCreateNextStream(string key, out Stream result, out Exception exception)
        {
            try
            {
                String path = string.IsNullOrEmpty(key) ? _filePath + _extension : $"{_filePath}_{key}{_extension}";
                result = new FileStream(path, _mode, _access);
                
                exception = null;
                return true;
            }
            catch (FileNotFoundException ex)
            {
                result = null;
                
                exception = ex;
                return false;
            }
        }
    }
}