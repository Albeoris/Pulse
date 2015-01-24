using System.Collections.Generic;
using System.Linq;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveBuilderNode
    {
        public readonly string Name;
        public readonly IArchiveListing Listing;       
        public readonly IArchiveEntry Entry;           
        public readonly Dictionary<string, UiArchiveBuilderNode> Childs = new Dictionary<string, UiArchiveBuilderNode>();

        public UiArchiveBuilderNode(string name)
        {
            Name = name;
        }

        public UiArchiveBuilderNode(string name, IArchiveListing listing)
            : this(name)
        {
            Listing = listing;
        }

        public UiArchiveBuilderNode(string name, IArchiveEntry entry)
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