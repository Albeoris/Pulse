using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class XgrArchiveListingReader
    {
        public static XgrArchiveListing Read(XgrArchiveAccessor accessor)
        {
            XgrArchiveListingReader reader = new XgrArchiveListingReader(accessor);
            return reader.Read();
        }

        private readonly XgrArchiveAccessor _accessor;

        private XgrArchiveListingReader(XgrArchiveAccessor accessor)
        {
            _accessor = Exceptions.CheckArgumentNull(accessor, "accessor");
        }

        public XgrArchiveListing Read()
        {
            using (Stream input = _accessor.ExtractIndices())
            {
                WpdHeader header = input.ReadContent<WpdHeader>();
                XgrArchiveListing result = new XgrArchiveListing(_accessor, header.Count);
                result.AddRange(header.Entries);
                return result;
            }
        }
    }
}