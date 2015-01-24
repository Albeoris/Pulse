using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class XgrArchiveExtractor : IProgressSender
    {
        private readonly XgrArchiveListing _listing;
        private readonly string _targetDir;
        private readonly Func<WpdEntry, IXgrArchiveEntryExtractor> _entryExtractorFactory;
        
        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public XgrArchiveExtractor(XgrArchiveListing listing, string targetDir, Func<WpdEntry, IXgrArchiveEntryExtractor> entryExtractorFactory)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _entryExtractorFactory = Exceptions.CheckArgumentNull(entryExtractorFactory, "entryExtractorFactory");

            _targetDir = Exceptions.CheckDirectoryNotFoundException(targetDir);
            _targetDir = Path.Combine(_targetDir, Path.ChangeExtension(_listing.Name, ".unpack"));
            Directory.CreateDirectory(_targetDir);
        }

        public void Extract()
        {
            long totalSize = 0;
            HashSet<string> paths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (WpdEntry entry in _listing)
            {
                totalSize += entry.Length;
                paths.Add(Path.GetDirectoryName(entry.Name));
            }
            ProgressTotalChanged.NullSafeInvoke(totalSize);

            using (Stream indices = _listing.Accessor.ExtractIndices())
            using (Stream content = _listing.Accessor.ExtractContent())
                foreach (WpdEntry entry in _listing)
                    Extract(entry, indices, content);
        }

        private void Extract(WpdEntry entry, Stream indices, Stream content)
        {
            IXgrArchiveEntryExtractor entryExtractor = _entryExtractorFactory(entry);
            entryExtractor.Extract(entry, indices, content, _targetDir, ProgressIncrement);
        }
    }
}