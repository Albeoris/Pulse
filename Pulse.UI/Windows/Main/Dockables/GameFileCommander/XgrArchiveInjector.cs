using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.FS
{
    // Obsolete
    public sealed class XgrArchiveInjector : IProgressSender
    {
        private readonly WpdArchiveListing _listing;
        private readonly bool? _compress;
        private Func<WpdEntry, XgrArchiveEntryInjectorWflContentPack> _entryInjectorFactory;

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncremented;

        public XgrArchiveInjector(WpdArchiveListing listing, bool? compress, Func<WpdEntry, XgrArchiveEntryInjectorWflContentPack> entryInjectorFactory)
        {
            _listing = Exceptions.CheckArgumentNull(listing, "listing");
            _entryInjectorFactory = Exceptions.CheckArgumentNull(entryInjectorFactory, "entryInjectorFactory");
            _compress = compress;
        }

        public void Inject()
        {
            ImgbArchiveAccessor accessor = _listing.Accessor;

            int indicesUncompressedSize = (int)accessor.XgrHeadersEntry.UncompressedSize;
            int contentUncompressedSize = (int)accessor.XgrContentEntry.UncompressedSize;

            long totalSize = indicesUncompressedSize + contentUncompressedSize;
            List<XgrArchiveEntryInjectorWflContentPack> injectors = new List<XgrArchiveEntryInjectorWflContentPack>(_listing.Count);
            foreach (WpdEntry entry in _listing)
            {
                XgrArchiveEntryInjectorWflContentPack entryInjector = _entryInjectorFactory(entry);
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
                using (Stream indicesInput = accessor.ExtractHeaders())
                using (Stream contentInput = accessor.ExtractContent())
                {
                    indicesInput.CopyToStream(indices, indicesUncompressedSize, buff, ProgressIncremented);
                    contentInput.CopyToStream(content, contentUncompressedSize, buff, ProgressIncremented);
                }

                foreach (XgrArchiveEntryInjectorWflContentPack injector in injectors)
                    injector.Inject(indices, content, ProgressIncremented);

                indices.Position = 0;
                WpdHeader header = new WpdHeader();
                WpdEntry[] entries = null;// _listing.FullListing.ToArray(); // todo
                header.Entries = entries;
                header.Count = entries.Length;
                header.WriteToStream(indices);

                indices.Position = 0;
                content.Position = 0;

                indicesUncompressedSize = (int)indices.Length;
                contentUncompressedSize = (int)content.Length;
                ProgressTotalChanged.NullSafeInvoke(totalSize + indicesUncompressedSize + contentUncompressedSize);

                //ArchiveEntryInjectorPack.Inject(accessor.Parent, accessor.XgrHeadersEntry, indices, indicesUncompressedSize, _compress, ProgressIncremented);
                //ArchiveEntryInjectorPack.Inject(accessor.Parent, accessor.XgrContentEntry, content, contentUncompressedSize, _compress, ProgressIncremented);
                //
                //ArchiveListingWriter.Write(_listing.ParentArchiveListing); // todo
            }
        }
    }
}