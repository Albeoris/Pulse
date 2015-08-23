using System;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiDataTableNode : UiLazyContainerNode
    {
        private readonly ArchiveListing _listing;
        private readonly UiArchiveExtension _extension;
        private readonly ArchiveEntry _indices;

        public UiDataTableNode(ArchiveListing listing, UiArchiveExtension extension, ArchiveEntry indices)
            : base(indices.Name, UiNodeType.DataTable)
        {
            _listing = listing;
            _extension = extension;
            _indices = indices;
        }

        protected override UiNode[] ExpandChilds()
        {
            switch (Name)
            {
                case "movie_items.win32.wdb":
                case "movie_items_us.win32.wdb":
                    return ExpandMovieChilds();
                default:
                    throw new NotImplementedException(Name);
            }
        }

        private UiNode[] ExpandMovieChilds()
        {
            DbArchiveAccessor dbAccessor = new DbArchiveAccessor(_listing, _indices);
            WdbMovieArchiveListing wpdListing = WdbMovieArchiveListingReader.Read(dbAccessor);

            UiNode[] result = new UiNode[wpdListing.Count];
            for (int i = 0; i < result.Length; i++)
            {
                WdbMovieEntry movieEntry = wpdListing[i];
                result[i] = new UiWdbMovieLeaf(movieEntry.Name, movieEntry, wpdListing) { Parent = this };
            }
            return result;
        }
    }
}