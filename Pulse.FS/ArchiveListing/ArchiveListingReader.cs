using System.Globalization;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingReader
    {
        private readonly Stream _input;

        public ArchiveListingReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
        }

        public ArchiveListing Read()
        {
            ArchiveListingHeader header = _input.ReadStruct<ArchiveListingHeader>();
            ArchiveListingEntryInfo[] entries = _input.ReadStructs<ArchiveListingEntryInfo>(header.EntriesCount);

            _input.Position = header.BlockOffset;
            ArchiveListingBlockInfo[] blocks = _input.ReadStructs<ArchiveListingBlockInfo>(header.BlocksCount);

            Encoding encoding = Encoding.GetEncoding(1251);

            byte[] buff = new byte[0];
            int blockLength = 0;

            ArchiveListing result = new ArchiveListing(header.EntriesCount);
            for (int currentBlock = -1, i = 0; i < header.EntriesCount; i++)
            {
                ArchiveListingEntryInfo entry = entries[i];
                if (entry.BlockNumber != currentBlock)
                {
                    currentBlock = entry.BlockNumber;
                    ArchiveListingBlockInfo block = blocks[currentBlock];
                    blockLength = block.UncompressedSize;

                    _input.Position = header.InfoOffset + block.Offset;
                    buff = ZLibHelper.Uncompress(_input, blockLength);
                }

                int infoLength;
                if (i < header.EntriesCount - 1)
                {
                    ArchiveListingEntryInfo next = entries[i + 1];
                    infoLength = next.BlockNumber == currentBlock ? next.Offset : blockLength - 4;
                }
                else
                {
                    infoLength = blockLength - 4;
                }
                infoLength = infoLength - entry.Offset - 1;

                string[] info = encoding.GetString(buff, entry.Offset, infoLength).Split(':');
                long sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                long uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                long compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                string name = info[3];

                result.Add(new ArchiveListingEntry(name, sector, compressedSize, uncompressedSize));
            }

            return result;
        }
    }
}