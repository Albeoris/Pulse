using System.IO;

namespace Pulse.FS
{
    public sealed class ImgbArchiveAccessor
    {
        public readonly ArchiveListing Parent;
        public readonly ArchiveEntry XgrHeadersEntry;
        public readonly ArchiveEntry XgrContentEntry;

        public ImgbArchiveAccessor(ArchiveListing parent, ArchiveEntry xgrHeadersEntry, ArchiveEntry xgrContentEntry)
        {
            Parent = parent;
            XgrHeadersEntry = xgrHeadersEntry;
            XgrContentEntry = xgrContentEntry;
        }

        public string Name
        {
            get { return XgrHeadersEntry.Name; }
        }

        public Stream ExtractHeaders()
        {
            return Parent.Accessor.ExtractBinary(XgrHeadersEntry);
        }

        public Stream ExtractContent()
        {
            return Parent.Accessor.ExtractBinary(XgrContentEntry);
        }

        public Stream RecreateIndices(int newSize)
        {
            return Parent.Accessor.OpenOrAppendBinary(XgrHeadersEntry, newSize);
        }

        public Stream RecreateContent(int newSize)
        {
            return Parent.Accessor.OpenOrAppendBinary(XgrContentEntry, newSize);
        }
    }
}