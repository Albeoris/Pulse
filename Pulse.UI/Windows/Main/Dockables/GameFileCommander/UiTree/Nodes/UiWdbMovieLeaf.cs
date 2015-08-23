using System;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWdbMovieLeaf : UiNode, IUiLeaf
    {
        public WdbMovieEntry Entry { get; private set; }
        public WdbMovieArchiveListing Listing { get; private set; }

        public UiWdbMovieLeaf(String name, WdbMovieEntry entry, WdbMovieArchiveListing listing)
            : base(name, UiNodeType.DataTableLeaf)
        {
            Entry = entry;
            Listing = listing;
        }
    }
}