using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class SeDbArchiveListingReader
    {
        public static SeDbArchiveListing Read(ImgbArchiveAccessor accessor)
        {
            SeDbArchiveListingReader reader = new SeDbArchiveListingReader(accessor);
            return reader.Read();
        }

        private readonly ImgbArchiveAccessor _accessor;

        private SeDbArchiveListingReader(ImgbArchiveAccessor accessor)
        {
            _accessor = Exceptions.CheckArgumentNull(accessor, "accessor");
        }

        public SeDbArchiveListing Read()
        {
            using (Stream input = _accessor.ExtractHeaders())
            {
                SeDbResHeader header = input.ReadContent<SeDbResHeader>();
                SeDbArchiveListing result = new SeDbArchiveListing(_accessor, header.Count);
                result.AddRange(header.Entries);
                return result;
            }
        }
    }
}