using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveNode : UiLazyContainerNode
    {
        private readonly ArchiveAccessor _accessor;
        private readonly ArchiveListing _parentListing;
        private ArchiveListing _listing;

        public UiArchiveNode(ArchiveAccessor accessor, ArchiveListing parentListing)
            : base(accessor.ListingEntry.Name, UiNodeType.Archive)
        {
            _accessor = accessor;
            _parentListing = parentListing;
        }

        protected override ImageSource GetIcon()
        {
            return Icons.DiskIcon;
        }

        protected override UiNode[] ExpandChilds()
        {
            UiChildPackageBuilder childPackages = new UiChildPackageBuilder(InteractionService.GameLocation.Provide().AreasDirectory);

            switch (InteractionService.GamePart)
            {
                case FFXIIIGamePart.Part1:
                    _listing = ArchiveListingReaderV1.Read(_accessor, null, null);
                    break;
                case FFXIIIGamePart.Part2:
                    _listing = ArchiveListingReaderV2.Read(_accessor, null, null);
                    break;
                default:
                    throw new NotSupportedException(InteractionService.GamePart.ToString());
            }

            _listing.Parent = _parentListing;
            
            HashSet<string> set = new HashSet<string>();
            Dictionary<string, UiNode> dic = new Dictionary<string, UiNode>(_listing.Count * 2);

            foreach (ArchiveEntry entry in _listing)
            {
                UiNode parent = this;

                string name;
                string entryPath = entry.Name.ToLowerInvariant();
                int index = entryPath.LastIndexOf(Path.AltDirectorySeparatorChar);
                if (index > 0)
                {
                    name = entryPath.Substring(index + 1);
                    string directoryPath = entryPath.Substring(0, index);
                    set.Add(directoryPath);
                    if (!dic.TryGetValue(directoryPath, out parent))
                    {
                        string directoryName = directoryPath;
                        int nameIndex = directoryPath.LastIndexOf(Path.AltDirectorySeparatorChar);
                        if (nameIndex > 0)
                            directoryName = directoryPath.Substring(nameIndex + 1);

                        parent = new UiContainerNode(directoryName, UiNodeType.Directory);
                        dic.Add(directoryPath, parent);
                    }
                }
                else
                {
                    name = entryPath;
                }

                if (!dic.ContainsKey(entryPath))
                    dic.Add(entryPath, new UiArchiveLeaf(name, entry, _listing) {Parent = parent});

                childPackages.TryAdd(_listing, entry, entryPath, name);
            }

            UiContainerNode packagesNode = childPackages.TryBuild();
            if (packagesNode != null)
            {
                packagesNode.Parent = this;
                dic.Add(packagesNode.Name, packagesNode);
            }

            string separator = Path.AltDirectorySeparatorChar.ToString();
            foreach (string dir in set)
            {
                UiNode parent = this;

                string[] parts = dir.Split(Path.AltDirectorySeparatorChar);
                for (int i = 0; i < parts.Length; i++)
                {
                    UiNode node;
                    string name = parts[i];
                    string path = String.Join(separator, parts, 0, i + 1);
                    if (!dic.TryGetValue(path, out node))
                    {
                        node = new UiContainerNode(name, UiNodeType.Directory);
                        dic.Add(path, node);
                    }
                    node.Parent = parent;
                    parent = node;
                }
            }

            UiNode[] result = EmptyChilds;
            foreach (IGrouping<UiNode, UiNode> group in dic.Values.GroupBy(n => n.Parent))
            {
                if (group.Key == this)
                {
                    result = group.ToArray();
                    continue;
                }

                UiContainerNode dir = ((UiContainerNode)group.Key);
                dir.SetChilds(group.ToArray());
            }

            foreach (UiNode node in dic.Values)
            {
                if (node.Type != UiNodeType.Directory)
                    continue;

                UiContainerNode container = (UiContainerNode)node;
                container.AbsorbSingleChildContainer();
            }

            return result;
        }
    }
}