using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiSeDbTableLeaf : UiNode, IUiLeaf
    {
        public SeDbResEntry Entry { get; private set; }
        public SeDbArchiveListing Listing { get; private set; }

        public UiSeDbTableLeaf(string name, SeDbResEntry entry, SeDbArchiveListing listing)
            : base(name, UiNodeType.FileTableLeaf)
        {
            Entry = entry;
            Listing = listing;
        }
    }
}