using System.Collections.Generic;
using System.Linq;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveBuilderNode
    {
        public string Name;
        public ArchiveListing Listing;
        public ArchiveListingEntry Entry;
        public readonly Dictionary<string, UiArchiveBuilderNode> Childs = new Dictionary<string, UiArchiveBuilderNode>();

        public UiArchiveBuilderNode(string name)
        {
            Name = name;
        }

        public UiArchiveBuilderNode(string name, ArchiveListing listing)
            : this(name)
        {
            Listing = listing;
        }

        public UiArchiveBuilderNode(string name, ArchiveListingEntry entry)
            : this(name)
        {
            Entry = entry;
        }

        public UiArchiveNode Commit(UiArchiveNode parent)
        {
            UiArchiveNode node = new UiArchiveNode
            {
                Name = Name,
                Listing = Listing,
                Entry = Entry,
                Parent = parent
            };

            node.Childs = Childs.Values.OrderBy(c => c.Name).Select(c => c.Commit(node)).ToArray();
            return node;
        }
    }
}