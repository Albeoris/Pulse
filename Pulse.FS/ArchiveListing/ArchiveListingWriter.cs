using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingWriter
    {
        public static void Write(ArchiveListing listing)
        {
            ArchiveListingWriter writer = new ArchiveListingWriter(listing);
            writer.Write();
        }

        private readonly ArchiveListing _listing;
        private readonly ArchiveAccessor _accessor;

        private ArchiveListingWriter(ArchiveListing listing)
        {
            _listing = listing;
            _accessor = _listing.Accessor;
        }

        public void Write()
        {
            using (MemoryStream headerBuff = new MemoryStream(32768))
            using (MemoryStream textBuff = new MemoryStream(32768))
            {
                ArchiveListingBlockInfo[] blocksInfo;
                ArchiveListingEntryInfoV1[] entriesInfoV1;
                ArchiveListingTextWriter textWriter = new ArchiveListingTextWriter(textBuff);
                textWriter.Write(_listing, out blocksInfo, out entriesInfoV1);

                for (int i = 0; i < entriesInfoV1.Length; i++)
                {
                    entriesInfoV1[i].UnknownNumber = _listing[i].UnknownNumber;
                    entriesInfoV1[i].UnknownValue = _listing[i].UnknownValue;
                }

                byte[] buff = new byte[8192];
                int blocksSize = (int)textBuff.Position;
                textBuff.Position = 0;

                ArchiveListingHeaderV1 header = new ArchiveListingHeaderV1
                {
                    EntriesCount = entriesInfoV1.Length,
                    BlockOffset = entriesInfoV1.Length * 8 + 12
                };
                header.InfoOffset = header.BlockOffset + blocksInfo.Length * 12;

                headerBuff.WriteStruct(header);
                foreach (ArchiveListingEntryInfoV1 entry in entriesInfoV1)
                    headerBuff.WriteStruct(entry);
                foreach (ArchiveListingBlockInfo block in blocksInfo)
                    headerBuff.WriteStruct(block);

                int hederSize = (int)headerBuff.Length;
                headerBuff.Position = 0;

                using (Stream output = _accessor.RecreateListing(hederSize + blocksSize))
                {
                    headerBuff.CopyToStream(output, hederSize, buff);
                    textBuff.CopyToStream(output, blocksSize, buff);
                }
            }
        }
    }
}