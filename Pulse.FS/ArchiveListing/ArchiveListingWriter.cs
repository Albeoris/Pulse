using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingWriter
    {
        public static void Write(ArchiveListing listing, ArchiveAccessor accessor)
        {
            ArchiveListingWriter writer = new ArchiveListingWriter(listing, accessor);
            writer.Write();
        }

        private readonly ArchiveListing _listing;
        private readonly ArchiveAccessor _accessor;

        private ArchiveListingWriter(ArchiveListing listing, ArchiveAccessor accessor)
        {
            _listing = listing;
            _accessor = accessor;
        }

        public void Write()
        {
            using (Stream output = _accessor.OpenCapacityListing())
            using (MemoryStream textBuff = new MemoryStream(32768))
            {
                ArchiveListingBlockInfo[] blocksInfo;
                ArchiveListingEntryInfo[] entriesInfo;
                ArchiveListingTextWriter textWriter = new ArchiveListingTextWriter(textBuff);
                textWriter.Write(_listing, out blocksInfo, out entriesInfo);

                for (int i = 0; i < entriesInfo.Length; i++)
                {
                    entriesInfo[i].UnknownNumber = _listing[i].UnknownNumber;
                    entriesInfo[i].UnknownValue = _listing[i].UnknownValue;
                }

                byte[] buff = new byte[8192];
                int blocksSize = (int)textBuff.Position;
                textBuff.Position = 0;

                ArchiveListingHeader header = new ArchiveListingHeader();
                header.EntriesCount = entriesInfo.Length;
                header.BlockOffset = entriesInfo.Length * 8 + 12;
                header.InfoOffset = header.BlockOffset + blocksInfo.Length * 12;

                output.WriteStruct(header);
                foreach (ArchiveListingEntryInfo entry in entriesInfo)
                    output.WriteStruct(entry);
                foreach (ArchiveListingBlockInfo block in blocksInfo)
                    output.WriteStruct(block);
                textBuff.CopyTo(output, blocksSize, buff);
            }
        }
    }
}