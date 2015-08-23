using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiFileTableNode : UiLazyContainerNode
    {
        private readonly ArchiveListing _listing;
        private readonly UiArchiveExtension _extension;
        private readonly ArchiveEntry _indices;
        private readonly ArchiveEntry _binary;

        public UiFileTableNode(ArchiveListing listing, UiArchiveExtension extension, ArchiveEntry indices, ArchiveEntry binary)
            : base(indices.Name, UiNodeType.FileTable)
        {
            _listing = listing;
            _extension = extension;
            _indices = indices;
            _binary = binary;
        }

        protected override UiNode[] ExpandChilds()
        {
            switch (_extension)
            {
                case UiArchiveExtension.Grs:
                case UiArchiveExtension.Xgr:
                case UiArchiveExtension.Xwb:
                case UiArchiveExtension.Xfv:
                    return ExpandWpdChilds();
                case UiArchiveExtension.Trb:
                    return ExpandTrbChilds();
                default:
                    throw new NotImplementedException(_extension.ToString());
            }
        }

        private UiNode[] ExpandWpdChilds()
        {
            ImgbArchiveAccessor imgbAccessor = new ImgbArchiveAccessor(_listing, _indices, _binary);
            WpdArchiveListing wpdListing = WpdArchiveListingReader.Read(imgbAccessor);

            UiNode[] result = new UiNode[wpdListing.Count];
            for (int i = 0; i < result.Length; i++)
            {
                WpdEntry xgrEntry = wpdListing[i];
                result[i] = new UiWpdTableLeaf(xgrEntry.Name, xgrEntry, wpdListing) {Parent = this};
            }
            return result;
        }

        private UiNode[] ExpandTrbChilds()
        {
            ImgbArchiveAccessor imgbAccessor = new ImgbArchiveAccessor(_listing, _indices, _binary);

            SeDbArchiveListing sedbListing = SeDbArchiveListingReader.Read(imgbAccessor);
            UiNode[] result = new UiNode[sedbListing.Count];
            int offset = sedbListing.Count * 16 + 0x40;
            using (Stream headers = imgbAccessor.ExtractHeaders())
            using (BinaryReader br = new BinaryReader(headers))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    SeDbResEntry entry = sedbListing[i];
                    String name = entry.Index.ToString();

                    SectionType type;
                    if (TryReadSectionType(br, offset, entry, out type))
                        name = name + "." + type.ToString().ToLower();

                    result[i] = new UiSeDbTableLeaf(name, entry, sedbListing) {Parent = this};
                }
            }
            return result;
        }

        private static bool TryReadSectionType(BinaryReader br, int offset, SeDbResEntry entry, out SectionType type)
        {
            br.BaseStream.SetPosition(entry.Offset + offset);

            int magic = br.ReadInt32();
            if (magic != SectionHeader.MagicNumber)
            {
                type = 0;
                return false;
            }

            type = (SectionType)br.ReadInt32();
            return true;
        }
    }
}