using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WpdArchiveListingReader
    {
        public static WpdArchiveListing Read(ImgbArchiveAccessor accessor)
        {
            WpdArchiveListingReader reader = new WpdArchiveListingReader(accessor);
            return reader.Read();
        }

        private readonly ImgbArchiveAccessor _accessor;

        private WpdArchiveListingReader(ImgbArchiveAccessor accessor)
        {
            _accessor = Exceptions.CheckArgumentNull(accessor, "accessor");
        }

        public WpdArchiveListing Read()
        {
            using (Stream input = _accessor.ExtractHeaders())
            {
                WpdHeader header = input.ReadContent<WpdHeader>();
                WpdArchiveListing result = new WpdArchiveListing(_accessor, header.Count);
                if (header.Entries != null)
                    result.AddRange(header.Entries);
                return result;
            }
        }
    }
}