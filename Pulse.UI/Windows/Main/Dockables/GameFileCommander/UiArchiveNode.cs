using System.Collections.Generic;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveNode
    {
        public string Name;
        public Dictionary<string, UiArchiveNode> Childs = new Dictionary<string, UiArchiveNode>();
        public ArchiveListingEntry Entry;

        public UiArchiveTreeViewItem GetTreeViewItem()
        {
            UiArchiveTreeViewItem item = new UiArchiveTreeViewItem {Header = Name, Tag = Entry};
            foreach (UiArchiveNode child in Childs.Values)
                item.Items.Add(child.GetTreeViewItem());
            return item;
        }
    }
}