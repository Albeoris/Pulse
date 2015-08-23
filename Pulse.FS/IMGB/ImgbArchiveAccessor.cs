using System.IO;

namespace Pulse.FS
{
    public sealed class ImgbArchiveAccessor : DbArchiveAccessor
    {
        public readonly ArchiveEntry ContentEntry;

        public ImgbArchiveAccessor(ArchiveListing parent, ArchiveEntry headersEntry, ArchiveEntry contentEntry)
            : base(parent, headersEntry)
        {
            ContentEntry = contentEntry;
        }

        public Stream ExtractContent()
        {
            return Parent.Accessor.ExtractBinary(ContentEntry);
        }

        public Stream RecreateContent(int newSize)
        {
            return Parent.Accessor.OpenOrAppendBinary(ContentEntry, newSize);
        }
    }
}