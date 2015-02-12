using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingReader
    {
        public static ArchiveListing[] Read(string zonesBinaryDirectory, ArchiveAccessor accessor)
        {
            ArchiveListingReader reader = new ArchiveListingReader(zonesBinaryDirectory, accessor);
            reader.Read();
            return reader._listings.ToArray();
        }

        private readonly ConcurrentBag<ArchiveAccessor> _accessors = new ConcurrentBag<ArchiveAccessor>();
        private readonly ConcurrentBag<ArchiveListing> _listings = new ConcurrentBag<ArchiveListing>();
        private readonly string _zonesBinaryDirectory;
        private long _counter;

        private ArchiveListingReader(string zonesBinaryDirectory, ArchiveAccessor accessor)
        {
            _zonesBinaryDirectory = zonesBinaryDirectory;
            _accessors.Add(accessor);
        }

        public void Read()
        {
            int count = Math.Max(Environment.ProcessorCount, 1);
            count = 1;
            using (Semaphore semaphore = new Semaphore(count, count))
            {
                do
                {
                    ArchiveAccessor acc;
                    while (_accessors.TryTake(out acc) && semaphore.WaitOne())
                    {
                        Interlocked.Increment(ref _counter);
                        ArchiveAccessor accessor = acc;
                        Task.Run(() => ReadInternal(accessor, semaphore));
                    }
                    Thread.Sleep(200);
                } while (Interlocked.Read(ref _counter) > 0 || _accessors.Count > 0);
            }
        }

        private void ReadInternal(ArchiveAccessor accessor, Semaphore semaphore)
        {
            using (Stream input = accessor.ExtractListing())
            {
                ArchiveListingHeader header = input.ReadStruct<ArchiveListingHeader>();
                ArchiveListingEntryInfoV1[] entries = input.ReadStructs<ArchiveListingEntryInfoV1>(header.EntriesCount);

                input.Position = header.BlockOffset;
                ArchiveListingBlockInfo[] blocks = input.ReadStructs<ArchiveListingBlockInfo>(header.BlocksCount);
             
                byte[] buff = new byte[0];
                int blockLength = 0;

                ArchiveListing result = new ArchiveListing(accessor, header.EntriesCount);
                for (int currentBlock = -1, i = 0; i < header.EntriesCount; i++)
                {
                    ArchiveListingEntryInfoV1 entryInfoV1 = entries[i];
                    if (entryInfoV1.BlockNumber != currentBlock)
                    {
                        currentBlock = entryInfoV1.BlockNumber;
                        ArchiveListingBlockInfo block = blocks[currentBlock];
                        blockLength = block.UncompressedSize;

                        input.Position = header.InfoOffset + block.Offset;
                        buff = ZLibHelper.Uncompress(input, blockLength);
                    }

                    int infoLength;
                    if (i < header.EntriesCount - 1)
                    {
                        ArchiveListingEntryInfoV1 next = entries[i + 1];
                        infoLength = next.BlockNumber == currentBlock ? next.Offset : blockLength - 4;
                    }
                    else
                    {
                        infoLength = blockLength - 4;
                    }
                    infoLength = infoLength - entryInfoV1.Offset - 1;

                    string[] info = Encoding.ASCII.GetString(buff, entryInfoV1.Offset, infoLength).Split(':');
                    long sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    long uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    long compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    string name = info[3];

                    ArchiveEntry entry = new ArchiveEntry(name, sector, compressedSize, uncompressedSize)
                    {
                        UnknownNumber = entryInfoV1.UnknownNumber,
                        UnknownValue = entryInfoV1.UnknownValue
                    };
                    result.Add(entry);

                    if (_zonesBinaryDirectory != null && name.StartsWith("zone/filelist"))
                    {
                        string binaryName = Path.Combine(_zonesBinaryDirectory, String.Format("white_{0}_img{1}.win32.bin", name.Substring(14, 5), name.EndsWith("2") ? "2" : string.Empty));
                        if (File.Exists(binaryName))
                            _accessors.Add(accessor.CreateChild(binaryName, entry));
                    }
                }
                _listings.Add(result);
            }
            semaphore.Release();
            Interlocked.Decrement(ref _counter);
        }

        //private void ReadInternal(ArchiveAccessor accessor, Semaphore semaphore)
        //{
        //    using (Stream input = accessor.ExtractListing())
        //    {
        //        ArchiveListingHeader header = input.ReadStruct<ArchiveListingHeader>();
        //        ArchiveListingEntryInfoV2[] entries = input.ReadStructs<ArchiveListingEntryInfoV2>(header.EntriesCount);

        //        input.Position = header.BlockOffset;
        //        ArchiveListingBlockInfo[] blocks = input.ReadStructs<ArchiveListingBlockInfo>(header.BlocksCount);
               
        //        byte[] buff = new byte[0];
        //        int blockLength = 0;

        //        ArchiveListing result = new ArchiveListing(accessor, header.EntriesCount);
        //        for (int currentBlock = -1, i = 0; i < header.EntriesCount; i++)
        //        {
        //            ArchiveListingEntryInfoV2 entryInfoV2 = entries[i];
        //            if (entryInfoV2.BlockNumber != currentBlock)
        //            {
        //                currentBlock = entryInfoV2.BlockNumber;
        //                ArchiveListingBlockInfo block = blocks[currentBlock];
        //                blockLength = block.UncompressedSize;

        //                input.Position = header.InfoOffset + block.Offset;
        //                buff = ZLibHelper.Uncompress(input, blockLength);
        //            }

        //            int infoLength;
        //            if (i < header.EntriesCount - 1)
        //            {
        //                ArchiveListingEntryInfoV2 next = entries[i + 1];
        //                infoLength = next.BlockNumber == currentBlock ? next.Offset : blockLength - 4;
        //            }
        //            else
        //            {
        //                infoLength = blockLength - 4;
        //            }
        //            infoLength = infoLength - entryInfoV2.Offset - 1;

        //            string[] info = Encoding.ASCII.GetString(buff, entryInfoV2.Offset, infoLength).Split(':');
        //            long sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        //            long uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        //            long compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        //            string name = info[3];

        //            ArchiveEntry entry = new ArchiveEntry(name, sector, compressedSize, uncompressedSize)
        //            {
        //                UnknownNumber = entryInfoV2.UnknownNumber,
        //                UnknownValue = entryInfoV2.UnknownValue,
        //                UnknownValueV2 = entryInfoV2.Unknown3
        //            };
        //            result.Add(entry);

        //            if (_zonesBinaryDirectory != null && name.StartsWith("zone/filelist"))
        //            {
        //                string binaryName = Path.Combine(_zonesBinaryDirectory, String.Format("white_{0}_img{1}.win32.bin", name.Substring(14, 5), name.EndsWith("2") ? "2" : string.Empty));
        //                if (File.Exists(binaryName))
        //                    _accessors.Add(accessor.CreateChild(binaryName, entry));
        //            }
        //        }
        //        _listings.Add(result);
        //    }
        //    semaphore.Release();
        //    Interlocked.Decrement(ref _counter);
        //}
    }
}