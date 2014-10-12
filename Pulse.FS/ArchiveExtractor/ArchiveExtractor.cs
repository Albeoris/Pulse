using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveExtractor : IProgressSender, IDisposable
    {
        private readonly MemoryMappedFile _mmf;
        private readonly ArchiveListing _listing;
        private readonly string _targetDir;

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public ArchiveExtractor(ArchiveListing listing, string targetDir)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _targetDir = Exceptions.CheckDirectoryNotFoundException(targetDir);
            _mmf = MemoryMappedFile.CreateFromFile(listing.BinaryFile);
        }

        public void Dispose()
        {
            _mmf.NullSafeDispose();
        }

        public void Extract()
        {
            long totalSize = 0;
            HashSet<string> paths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (ArchiveListingEntry entry in _listing)
            {
                totalSize += entry.UncompressedSize;
                paths.Add(Path.GetDirectoryName(entry.Name));
            }

            foreach (string path in paths)
                Directory.CreateDirectory(Path.Combine(_targetDir, path));

            ProgressTotalChanged.NullSafeInvoke(totalSize);
            Parallel.ForEach(_listing, Extract);
        }

        private void Extract(ArchiveListingEntry entry)
        {
            string fullPath = Path.Combine(_targetDir, entry.Name);
            using (MemoryMappedViewStream input = _mmf.CreateViewStream(entry.Offset, entry.Size))
            using (FileStream output = File.Create(fullPath, 32768))
            {
                if (entry.Size == entry.UncompressedSize)
                {
                    input.CopyTo(output, 32 * 1024);
                    ProgressIncrement(entry.UncompressedSize);
                }
                else
                {
                    ZLibHelper.Uncompress(input, output, (int)entry.UncompressedSize, ProgressIncrement);
                }
            }
        }
    }
}