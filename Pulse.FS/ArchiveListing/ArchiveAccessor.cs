using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveAccessor
    {
        private int _level;

        private readonly SharedMemoryMappedFile _binaryFile;
        private readonly SharedMemoryMappedFile _listingFile;
        public readonly ArchiveEntry ListingEntry;

        public ArchiveAccessor(string binaryFile, string listingFile)
        {
            _binaryFile = new SharedMemoryMappedFile(binaryFile);
            _listingFile = new SharedMemoryMappedFile(listingFile);

            FileInfo listingFileInfo = new FileInfo(listingFile);
            ListingEntry = new ArchiveEntry(listingFileInfo.Name, 0, listingFileInfo.Length, listingFileInfo.Length);
        }

        private ArchiveAccessor(SharedMemoryMappedFile binaryFile, SharedMemoryMappedFile listingFile, ArchiveEntry listingEntry)
        {
            _binaryFile = binaryFile;
            _listingFile = listingFile;
            ListingEntry = listingEntry;
        }

        public ArchiveAccessor CreateDescriptor(ArchiveEntry entry)
        {
            ArchiveAccessor result = new ArchiveAccessor(null, _binaryFile, entry);
            result._level = _level + 1;
            return result;
        }

        public ArchiveAccessor CreateDescriptor(string binaryFile, ArchiveEntry entry)
        {
            ArchiveAccessor result = new ArchiveAccessor(new SharedMemoryMappedFile(binaryFile), _binaryFile, entry);
            result._level = _level + 1;
            return result;
        }

        public int Level => _level;
        public bool IsDescriptor => _binaryFile == null;

        public Stream ExtractListing()
        {
            Stream compressed = _listingFile.CreateViewStream(ListingEntry.Offset, ListingEntry.Size, MemoryMappedFileAccess.Read);
            return BackgroundExtractIfCompressed(compressed, ListingEntry);
        }

        public Stream RecreateListing(int newSize)
        {
            try
            {
                if (_level == 0)
                    return _listingFile.RecreateFile();

                long capacity = MathEx.RoundUp(ListingEntry.Size, 0x800);
                if (newSize <= capacity)
                    return _listingFile.CreateViewStream(ListingEntry.Offset, newSize, MemoryMappedFileAccess.Write);

                long offset;
                Stream result = _listingFile.IncreaseSize(MathEx.RoundUp(newSize, 0x800), out offset);
                ListingEntry.Sector = (int)(offset / 0x800);
                return result;
            }
            finally
            {
                ListingEntry.Size = newSize;
                ListingEntry.UncompressedSize = newSize;
            }
        }

        public Stream OpenBinary(ArchiveEntry entry)
        {
            return _binaryFile.CreateViewStream(entry.Offset, entry.Size, MemoryMappedFileAccess.ReadWrite);
        }

        public Stream ExtractBinary(ArchiveEntry entry)
        {
            Stream compressed = _binaryFile.CreateViewStream(entry.Offset, entry.Size, MemoryMappedFileAccess.Read);
            return BackgroundExtractIfCompressed(compressed, entry);
        }

        public Stream OpenOrAppendBinary(ArchiveEntry entry, int newSize)
        {
            try
            {
                long capacity = MathEx.RoundUp(entry.Size, 0x800);
                if (newSize <= capacity)
                    return _binaryFile.CreateViewStream(entry.Offset, newSize, MemoryMappedFileAccess.Write);

                long offset;
                Stream result = _binaryFile.IncreaseSize(MathEx.RoundUp(newSize, 0x800), out offset);
                entry.Sector = (int)(offset / 0x800);
                return result;
            }
            finally
            {
                entry.Size = newSize;
            }
        }

        public static Stream BackgroundExtractIfCompressed(Stream input, ArchiveEntry entry)
        {
            if (!entry.IsCompressed)
                return input;

            int uncompressedSize = (int)entry.UncompressedSize;
            if (uncompressedSize == 0)
                return new MemoryStream(0);

            Stream writer, reader;
            Flute.CreatePipe(uncompressedSize, out writer, out reader);

            //if (!ThreadPool.QueueUserWorkItem((o) => ZLibHelper.UncompressAndDisposeStreams(input, writer, uncompressedSize, CancellationToken.None)))
                ThreadHelper.StartBackground("UncompressAndDisposeSourceAsync", () => ZLibHelper.UncompressAndDisposeStreams(input, writer, uncompressedSize, CancellationToken.None));
            return reader;
        }

        public void OnWritingCompleted(ArchiveEntry entry, MemoryStream ms, bool? compression)
        {
            ms.Position = 0;

            int compressedSize = 0;
            int uncompressedSize = (int)ms.Length;
            byte[] copyBuff = new byte[Math.Min(uncompressedSize, 32 * 1024)];

            try
            {
                bool compress = uncompressedSize > 256 && (compression ?? entry.IsCompressed);
                if (compress)
                {
                    using (SafeUnmanagedArray buff = new SafeUnmanagedArray(uncompressedSize + 256))
                    using (UnmanagedMemoryStream buffStream = buff.OpenStream(FileAccess.ReadWrite))
                    {
                        compressedSize = ZLibHelper.Compress(ms, buffStream, uncompressedSize);
                        if (uncompressedSize - compressedSize > 256)
                        {
                            using (Stream output = OpenOrAppendBinary(entry, compressedSize))
                            {
                                buffStream.Position = 0;
                                buffStream.CopyToStream(output, compressedSize, copyBuff);
                            }
                            return;
                        }
                    }
                }

                ms.Position = 0;
                compressedSize = uncompressedSize;
                using (Stream output = OpenOrAppendBinary(entry, uncompressedSize))
                    ms.CopyToStream(output, uncompressedSize, copyBuff);
            }
            finally
            {
                entry.Size = compressedSize;
                entry.UncompressedSize = uncompressedSize;
            }
        }
    }
}