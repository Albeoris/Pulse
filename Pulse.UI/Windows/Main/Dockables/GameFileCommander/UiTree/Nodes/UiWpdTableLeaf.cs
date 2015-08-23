using System.Collections.Generic;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdTableLeaf : UiNode, IUiLeaf
    {
        public WpdEntry Entry { get; private set; }
        public WpdArchiveListing Listing { get; private set; }

        public UiWpdTableLeaf(string name, WpdEntry entry, WpdArchiveListing listing)
            : base(name, UiNodeType.FileTableLeaf)
        {
            Entry = entry;
            Listing = listing;
        }
    }
}