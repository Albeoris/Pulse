using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveExtractor : IProgressSender
    {
        private readonly ArchiveListing _listing;
        private readonly string _targetDir;

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public ArchiveExtractor(ArchiveListing listing, string targetDir)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _targetDir = Exceptions.CheckDirectoryNotFoundException(targetDir);
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
            using (Stream input = _listing.Accessor.OpenBinary(entry))
            using (FileStream output = File.Create(fullPath, 32768))
            {
                ArchiveEntryExtractor entryExtractor = new ArchiveEntryExtractor(entry, input, output);
                entryExtractor.ProgressIncrement += ProgressIncrement;
                entryExtractor.Extract();
            }
        }
    }
}