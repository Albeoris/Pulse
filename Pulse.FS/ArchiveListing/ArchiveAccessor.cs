using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        public ArchiveAccessor CreateChild(string binaryFile, ArchiveEntry entry)
        {
            ArchiveAccessor result = new ArchiveAccessor(new SharedMemoryMappedFile(binaryFile), _binaryFile, entry);
            result._level++;
            return result;
        }

        public Stream ExtractListing()
        {
            Stream compressed = _listingFile.CreateViewStream(ListingEntry.Offset, ListingEntry.Size);
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
                    return _listingFile.CreateViewStream(ListingEntry.Offset, newSize);

                long offset;
                Stream result = _listingFile.IncreaseSize(MathEx.RoundUp(newSize, 0x800), out offset);
                ListingEntry.Sector = (int)(offset / 0x800);
                return result;
            }
            finally
            {
                ListingEntry.Size = newSize;
            }
        }

        public Stream OpenBinary(ArchiveEntry entry)
        {
            return _binaryFile.CreateViewStream(entry.Offset, entry.Size);
        }

        public Stream ExtractBinary(ArchiveEntry entry)
        {
            Stream compressed = _binaryFile.CreateViewStream(entry.Offset, entry.Size);
            return BackgroundExtractIfCompressed(compressed, entry);
        }

        public Stream OpenOrAppendBinary(ArchiveEntry entry, int newSize)
        {
            try
            {
                long capacity = MathEx.RoundUp(entry.Size, 0x800);
                if (newSize <= capacity)
                    return _binaryFile.CreateViewStream(entry.Offset, newSize);

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

            ProxyMemoryStream proxy = new ProxyMemoryStream(uncompressedSize);
            DisposableStream result = new DisposableStream(proxy);

            CancellationTokenSource cts = new CancellationTokenSource();
            Task uncompressingTask = ZLibHelper.UncompressAndDisposeSourceAsync(input, proxy, uncompressedSize, cts.Token);
            result.BeforeDispose.Add(new DisposableAction(cts.Dispose, true));
            result.BeforeDispose.Add(new DisposableAction(() => uncompressingTask.CancelAndWait(cts, 5000), true));
            return result;
        }
    }
}