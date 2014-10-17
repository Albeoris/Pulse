using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveAccessor
    {
        private readonly SharedMemoryMappedFile _binaryFile;
        private readonly SharedMemoryMappedFile _listingFile;
        public readonly ArchiveListingEntry ListingEntry;

        public ArchiveAccessor(string binaryFile, string listingFile)
        {
            _binaryFile = new SharedMemoryMappedFile(binaryFile);
            _listingFile = new SharedMemoryMappedFile(listingFile);

            FileInfo listingFileInfo = new FileInfo(listingFile);
            ListingEntry = new ArchiveListingEntry(listingFileInfo.Name, 0, listingFileInfo.Length, listingFileInfo.Length);
        }

        private ArchiveAccessor(SharedMemoryMappedFile binaryFile, SharedMemoryMappedFile listingFile, ArchiveListingEntry entry)
        {
            _binaryFile = binaryFile;
            _listingFile = listingFile;
            ListingEntry = entry;
        }

        public ArchiveAccessor CreateChild(ArchiveListingEntry listingEntry)
        {
            return new ArchiveAccessor(_binaryFile, _binaryFile, listingEntry);
        }

        public ArchiveAccessor CreateChild(string binaryFile, ArchiveListingEntry listingEntry)
        {
            return new ArchiveAccessor(new SharedMemoryMappedFile(binaryFile), _binaryFile, listingEntry);
        }

        public Stream OpenListing()
        {
            return _listingFile.CreateViewStream(ListingEntry.Offset, ListingEntry.Size);
        }

        public Stream OpenBinary(ArchiveListingEntry entry)
        {
            return _binaryFile.CreateViewStream(entry.Offset, entry.Size);
        }
    }
}