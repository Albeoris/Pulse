using System;
using System.IO;

namespace Pulse.FS
{
    public sealed class ArchiveEntryExtractor
    {
        private readonly ArchiveListingEntry _entry;
        private readonly Stream _input;
        private readonly Stream _output;

        public event Action<long> ProgressIncrement;

        public ArchiveEntryExtractor(ArchiveListingEntry entry, Stream input, Stream output)
        {
            _entry = entry;
            _input = input;
            _output = output;
        }

        public void Extract()
        {
            if (_entry.Size == _entry.UncompressedSize)
            {
                _input.CopyTo(_output, 32 * 1024);
                ProgressIncrement(_entry.UncompressedSize);
            }
            else
            {
                ZLibHelper.Uncompress(_input, _output, (int)_entry.UncompressedSize, ProgressIncrement);
            }
        }
    }
}