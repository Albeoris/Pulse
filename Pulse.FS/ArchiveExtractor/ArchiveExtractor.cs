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
        private readonly Func<ArchiveEntry, IArchiveEntryExtractor> _entryExtractorFactory;
        
        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public ArchiveExtractor(ArchiveListing listing, string targetDir, Func<ArchiveEntry, IArchiveEntryExtractor> entryExtractorFactory)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _targetDir = Exceptions.CheckDirectoryNotFoundException(targetDir);
            _entryExtractorFactory = Exceptions.CheckArgumentNull(entryExtractorFactory, "entryExtractorFactory");
        }

        public void Extract()
        {
            long totalSize = 0;
            HashSet<string> paths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (ArchiveEntry entry in _listing)
            {
                totalSize += entry.UncompressedSize;
                paths.Add(Path.GetDirectoryName(entry.Name));
            }

            foreach (string path in paths)
                Directory.CreateDirectory(Path.Combine(_targetDir, path));

            ProgressTotalChanged.NullSafeInvoke(totalSize);
            Parallel.ForEach(_listing, Extract);
        }

        private void Extract(ArchiveEntry entry)
        {
            IArchiveEntryExtractor entryExtractor = _entryExtractorFactory(entry);
            entryExtractor.Extract(_listing.Accessor, entry, _targetDir, ProgressIncrement);
        }
    }
}