using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.FS
{
    public sealed class XgrArchiveInjector : IProgressSender
    {
        private readonly XgrArchiveListing _listing;
        private readonly bool? _compress;
        private Func<WpdEntry, IXgrArchiveEntryInjector> _entryInjectorFactory;

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public XgrArchiveInjector(XgrArchiveListing listing, bool? compress, Func<WpdEntry, IXgrArchiveEntryInjector> entryInjectorFactory)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _entryInjectorFactory = Exceptions.CheckArgumentNull(entryInjectorFactory, "entryInjectorFactory");
            _compress = compress;
        }

        public void Inject()
        {
            XgrArchiveAccessor accessor = _listing.Accessor;

            int indicesUncompressedSize = (int)accessor.XgrIndicesEntry.UncompressedSize;
            int contentUncompressedSize = (int)accessor.XgrContentEntry.UncompressedSize;

            long totalSize = indicesUncompressedSize + contentUncompressedSize;
            List<IXgrArchiveEntryInjector> injectors = new List<IXgrArchiveEntryInjector>(_listing.Count);
            foreach (WpdEntry entry in _listing)
            {
                IXgrArchiveEntryInjector entryInjector = _entryInjectorFactory(entry);
                if (entryInjector == null)
                    continue;

                totalSize += entryInjector.CalcSize();
                injectors.Add(entryInjector);
            }
            ProgressTotalChanged.NullSafeInvoke(totalSize);

            byte[] buff = new byte[32 * 1024];
            using (MemoryStream indices = new MemoryStream(indicesUncompressedSize))
            using (MemoryStream content = new MemoryStream(contentUncompressedSize))
            {
                using (Stream indicesInput = accessor.ExtractIndices())
                using (Stream contentInput = accessor.ExtractContent())
                {
                    indicesInput.CopyTo(indices, indicesUncompressedSize, buff, ProgressIncrement);
                    contentInput.CopyTo(content, contentUncompressedSize, buff, ProgressIncrement);
                }

                foreach (IXgrArchiveEntryInjector injector in injectors)
                    injector.Inject(indices, content, ProgressIncrement);

                indices.SetWritePosition(0);
                WpdHeader header = new WpdHeader();
                WpdEntry[] entries = _listing.FullListing.ToArray();
                header.Entries = entries;
                header.Count = entries.Length;
                header.WriteToStream(indices);

                indices.SetReadPosition(0);
                content.SetReadPosition(0);

                indicesUncompressedSize = (int)indices.Length;
                contentUncompressedSize = (int)content.Length;
                ProgressTotalChanged.NullSafeInvoke(totalSize + indicesUncompressedSize + contentUncompressedSize);

                ArchiveEntryInjectorPack.Inject(accessor.Parent, accessor.XgrIndicesEntry, indices, indicesUncompressedSize, _compress, ProgressIncrement);
                ArchiveEntryInjectorPack.Inject(accessor.Parent, accessor.XgrContentEntry, content, contentUncompressedSize, _compress, ProgressIncrement);

                ArchiveListingWriter.Write(_listing.ParentArchiveListing);
            }
        }
    }
}