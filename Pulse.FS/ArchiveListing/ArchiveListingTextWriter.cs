using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingTextWriter
    {
        private readonly Stream _output;

        public ArchiveListingTextWriter(Stream output)
        {
            _output = output;
        }

        public void Write(ArchiveListing listing, out ArchiveListingBlockInfo[] blocksInfo, out ArchiveListingEntryInfo[] entriesInfo)
        {
            using (MemoryStream ms = new MemoryStream(32768))
            {
                int blockNumber = 0, unpackedBlockOffset = 0;
                entriesInfo = new ArchiveListingEntryInfo[listing.Count];
                List<ArchiveListingBlockInfo> blocks = new List<ArchiveListingBlockInfo>(128);
                using (FormattingStreamWriter sw = new FormattingStreamWriter(ms, Encoding.ASCII, 4096, true, CultureInfo.InvariantCulture))
                {
                    sw.AutoFlush = true;
                    for (int i = 0; i < listing.Count; i++)
                    {
                        if (i == 14106)
                            Console.WriteLine();
                        ArchiveListingEntry entry = listing[i];
                        entriesInfo[i] = new ArchiveListingEntryInfo {BlockNumber = (short)(ms.Position / 8192)};

                        if (blockNumber != entriesInfo[i].BlockNumber)
                        {
                            sw.Write("end\0");
                            int blockSize = (int)(ms.Position - unpackedBlockOffset);
                            ms.Position = unpackedBlockOffset;
                            ArchiveListingBlockInfo block = new ArchiveListingBlockInfo {Offset = (int)_output.Position, UncompressedSize = blockSize};
                            block.CompressedSize = ZLibHelper.Compress(ms, _output, block.UncompressedSize);
                            blocks.Add(block);

                            blockNumber++;
                            unpackedBlockOffset = (int)ms.Position;
                            sw.Write("{0:x}:{1:x}:{2:x}:{3}\0", entry.Sector, entry.UncompressedSize, entry.Size, entry.Name);
                        }
                        else if (i == listing.Count - 1)
                        {
                            entriesInfo[i].Offset = (short)(ms.Position - unpackedBlockOffset);
                            sw.Write("{0:x}:{1:x}:{2:x}:{3}\0end\0", entry.Sector, entry.UncompressedSize, entry.Size, entry.Name);
                            int blockSize = (int)(ms.Position - unpackedBlockOffset);
                            ms.Position = unpackedBlockOffset;
                            ArchiveListingBlockInfo block = new ArchiveListingBlockInfo {Offset = (int)_output.Position, UncompressedSize = blockSize};
                            block.CompressedSize = ZLibHelper.Compress(ms, _output, block.UncompressedSize);
                            blocks.Add(block);
                        }
                        else
                        {
                            entriesInfo[i].Offset = (short)(ms.Position - unpackedBlockOffset);
                            sw.Write("{0:x}:{1:x}:{2:x}:{3}\0", entry.Sector, entry.UncompressedSize, entry.Size, entry.Name);
                        }

                        //lengths[i] = (int)_output.Position - offsets[i];
                        //blockSize += lengths[i];
                    }
                }
                blocksInfo = blocks.ToArray();
            }
        }
    }
}