using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class SeDbArchiveListing : List<SeDbResEntry>, IArchiveListing
    {
        public readonly ImgbArchiveAccessor Accessor;

        public SeDbArchiveListing(ImgbArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public string Name
        {
            get { return Accessor.Name; }
        }
    }
}