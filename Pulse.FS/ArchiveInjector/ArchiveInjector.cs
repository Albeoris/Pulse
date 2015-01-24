using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveInjector : IProgressSender
    {
        private readonly ArchiveListing _listing;
        private readonly bool? _compress;
        private Func<ArchiveEntry, IArchiveEntryInjector> _entryInjectorFactory;

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public ArchiveInjector(ArchiveListing listing, bool? compress, Func<ArchiveEntry, IArchiveEntryInjector> entryInjectorFactory)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _entryInjectorFactory = Exceptions.CheckArgumentNull(entryInjectorFactory, "entryInjectorFactory");
            _compress = compress;
        }

        public void Inject()
        {
            long totalSize = 0;
            List<IArchiveEntryInjector> injectors = new List<IArchiveEntryInjector>(_listing.Count);
            foreach (ArchiveEntry entry in _listing)
            {
                IArchiveEntryInjector entryInjector = _entryInjectorFactory(entry);
                if (entryInjector == null)
                    continue;

                totalSize += entryInjector.CalcSize();
                injectors.Add(entryInjector);
            }

            ProgressTotalChanged.NullSafeInvoke(totalSize);

            foreach (IArchiveEntryInjector injector in injectors)
                injector.Inject(_listing.Accessor, _compress, ProgressIncrement);

            ArchiveListingWriter.Write(_listing.FullListing ?? _listing);
        }
    }
}