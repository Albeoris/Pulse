using System.IO;

namespace Pulse.FS
{
    public class DbArchiveAccessor
    {
        public readonly ArchiveListing Parent;
        public readonly ArchiveEntry HeadersEntry;

        public DbArchiveAccessor(ArchiveListing parent, ArchiveEntry headersEntry)
        {
            Parent = parent;
            HeadersEntry = headersEntry;
        }

        public string Name
        {
            get { return HeadersEntry.Name; }
        }

        public Stream ExtractHeaders()
        {
            return Parent.Accessor.ExtractBinary(HeadersEntry);
        }

        public Stream RecreateHeaders(int newSize)
        {
            return Parent.Accessor.OpenOrAppendBinary(HeadersEntry, newSize);
        }
    }
}