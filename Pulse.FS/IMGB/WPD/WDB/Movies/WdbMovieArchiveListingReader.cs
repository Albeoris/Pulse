using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WdbMovieArchiveListingReader
    {
        public static WdbMovieArchiveListing Read(DbArchiveAccessor accessor)
        {
            WdbMovieArchiveListingReader reader = new WdbMovieArchiveListingReader(accessor);
            return reader.Read();
        }

        private readonly DbArchiveAccessor _accessor;

        private WdbMovieArchiveListingReader(DbArchiveAccessor accessor)
        {
            _accessor = Exceptions.CheckArgumentNull(accessor, "accessor");
        }

        public WdbMovieArchiveListing Read()
        {
            using (Stream input = _accessor.ExtractHeaders())
            {
                WdbMovieHeader header = input.ReadContent<WdbMovieHeader>();
                WdbMovieArchiveListing result = new WdbMovieArchiveListing(_accessor, header.Count);
                if (header.Movies != null)
                    result.AddRange(header.Movies);
                return result;
            }
        }
    }
}