using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingReaderV1 : IDisposable
    {
        public static ArchiveListing Read(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            using (ArchiveListingReaderV1 reader = new ArchiveListingReaderV1(accessor, progressIncrement, progressTotalChanged))
                return reader.Read();
        }

        private readonly ArchiveAccessor _accessor;
        private readonly Stream _input;
        private readonly Action<long> _progressIncrement;
        private readonly Action<long> _progressTotalChanged;

        private ArchiveListingReaderV1(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            _accessor = accessor;
            _progressIncrement = progressIncrement;
            _progressTotalChanged = progressTotalChanged;
            _input = accessor.ExtractListing();
        }

        public void Dispose()
        {
            _input.SafeDispose();
        }

        public ArchiveListing Read()
        {
            ArchiveListingHeaderV1 header = _input.ReadStruct<ArchiveListingHeaderV1>();
            _progressTotalChanged.NullSafeInvoke(header.EntriesCount);

            ArchiveListingEntryInfoV1[] entries = _input.ReadStructs<ArchiveListingEntryInfoV1>(header.EntriesCount);

            ArchiveListingCompressedData data = new ArchiveListingCompressedData(header);
            data.ReadFromStream(_input);

            ArchiveListing result = new ArchiveListing(_accessor, header);
            ParseEntries(entries, data, result);
            return result;
        }

        private void ParseEntries(ArchiveListingEntryInfoV1[] entries, ArchiveListingCompressedData data, ArchiveListing result)
        {
            byte[] buff = new byte[0];

            for (int currentBlock = -1, i = 0; i < entries.Length; i++)
            {
                ArchiveListingEntryInfoV1 entryInfoV1 = entries[i];
                if (entryInfoV1.BlockNumber != currentBlock)
                {
                    currentBlock = entryInfoV1.BlockNumber;
                    buff = data.AcquireData(currentBlock);
                }

                string name;
                long sector, uncompressedSize, compressedSize;
                ParseInfo(entryInfoV1, buff, out sector, out uncompressedSize, out compressedSize, out name);

                ArchiveEntry entry = new ArchiveEntry(name, sector, compressedSize, uncompressedSize)
                {
                    UnknownNumber = entryInfoV1.UnknownNumber,
                    UnknownValue = entryInfoV1.UnknownValue
                };

                result.Add(entry);
                _progressIncrement.NullSafeInvoke(1);
            }
        }

        private void ParseInfo(ArchiveListingEntryInfoV1 entryInfo, byte[] uncompressedData, out long sector, out long uncompressedSize, out long compressedSize, out string name)
        {
            string[] info;
            unsafe
            {
                fixed (byte* ptr = &uncompressedData[entryInfo.Offset])
                {
                    string str = new string((sbyte*)ptr);
                    info = str.Split(':');
                }
            }

            if (info.Length < 4)
            {
                name = String.Join(":", info);
                sector = -1;
                uncompressedSize = -1;
                compressedSize = -1;
            }
            else
            {
                sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                name = info[3];
            }
        }
    }
}