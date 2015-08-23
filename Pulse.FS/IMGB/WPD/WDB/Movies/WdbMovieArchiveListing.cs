using System;
using System.Collections.Generic;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WdbMovieArchiveListing : List<WdbMovieEntry>, IArchiveListing
    {
        public readonly DbArchiveAccessor Accessor;

        public WdbMovieArchiveListing(DbArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public string Name
        {
            get { return Accessor.Name; }
        }

        public string ExtractionSubpath
        {
            get { return PathEx.ChangeMultiDotExtension(Name, ".unpack"); }
        }

        public String PackagePostfix
        {
            get { return PathEx.ChangeMultiDotExtension(Name, null).EndsWith("_us") ? "_us" : String.Empty; }
        }
    }
}