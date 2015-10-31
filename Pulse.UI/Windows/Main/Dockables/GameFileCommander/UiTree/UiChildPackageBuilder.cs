using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiChildPackageBuilder
    {
        private readonly ConcurrentDictionary<UiArchiveExtension, ConcurrentBag<UiNode>> _nodes = new ConcurrentDictionary<UiArchiveExtension, ConcurrentBag<UiNode>>();
        private readonly ConcurrentDictionary<string, Pair<ArchiveEntry, ArchiveEntry>> _pairs = new ConcurrentDictionary<string, Pair<ArchiveEntry, ArchiveEntry>>();

        private readonly string _areasDirectory;

        public UiChildPackageBuilder(string areasDirectory)
        {
            _areasDirectory = areasDirectory;
        }

        public bool TryAdd(ArchiveListing listing, ArchiveEntry entry, string entryPath, string entryName)
        {
            if (TryAddZoneListing(listing, entry, entryPath))
                return true;

            if (TryAddMoviesListing(listing, entry, entryName))
                return true;

            if (TryAddImgbPair(listing, entry, entryPath, entryName))
                return true;

            return false;
        }

        public UiContainerNode TryBuild()
        {
            if (_nodes.Count == 0)
                return null;

            int counter = 0;
            UiContainerNode result = new UiContainerNode(Lang.Dockable.GameFileCommander.ArchivesNode, UiNodeType.Group);
            UiNode[] extensions = new UiNode[_nodes.Count];
            foreach (KeyValuePair<UiArchiveExtension, ConcurrentBag<UiNode>> pair in _nodes)
            {
                UiContainerNode extensionNode = BuildExtensionNode(pair.Key, pair.Value);
                extensionNode.Parent = result;
                extensions[counter++] = extensionNode;
            }
            result.SetChilds(extensions);
            return result;
        }

        private UiContainerNode BuildExtensionNode(UiArchiveExtension key, ConcurrentBag<UiNode> entries)
        {
            string separator = Path.AltDirectorySeparatorChar.ToString();

            UiContainerNode extensionNode = new UiContainerNode(key.ToString().ToUpper(), UiNodeType.Group);
            Dictionary<string, UiContainerNode> dirs = new Dictionary<string, UiContainerNode>(entries.Count);
            foreach (UiNode leaf in entries)
            {
                UiNode parent = extensionNode;
                string[] path = leaf.Name.ToLowerInvariant().Split(Path.AltDirectorySeparatorChar);
                for (int i = 0; i < path.Length - 1; i++)
                {
                    UiContainerNode directory;
                    string directoryName = path[i];
                    string directoryPath = String.Join(separator, path, 0, i + 1);
                    if (!dirs.TryGetValue(directoryPath, out directory))
                    {
                        directory = new UiContainerNode(directoryName, UiNodeType.Directory) {Parent = parent};
                        dirs.Add(directoryPath, directory);
                    }
                    parent = directory;
                }
                leaf.Parent = parent;
                leaf.Name = path[path.Length - 1];
            }

            //foreach (IGrouping<UiNode, UiNode> leafs in entries.GroupBy(e => e.Parent))
            //    ((UiContainerNode)leafs.Key).SetChilds(leafs.ToArray());

            UiNode[] childs = null;
            foreach (IGrouping<UiNode, UiNode> leafs in dirs.Values.Union(entries).GroupBy(e => e.Parent))
            {
                if (leafs.Key == extensionNode)
                {
                    childs = leafs.ToArray();
                    continue;
                }

                ((UiContainerNode)leafs.Key).SetChilds(leafs.ToArray());
            }

            foreach (UiContainerNode node in dirs.Values)
                node.AbsorbSingleChildContainer();

            extensionNode.SetChilds(childs);
            return extensionNode;
        }

        private ConcurrentBag<UiNode> ProvideRootNodeChilds(UiArchiveExtension extension)
        {
            return _nodes.GetOrAdd(extension, e => new ConcurrentBag<UiNode>());
        }

        private Pair<ArchiveEntry, ArchiveEntry> ProvidePair(string entryPathWithoutExtension)
        {
            return _pairs.GetOrAdd(entryPathWithoutExtension, p => new Pair<ArchiveEntry, ArchiveEntry>());
        }

        private bool TryAddZoneListing(ArchiveListing parentListing, ArchiveEntry entry, string entryPath)
        {
            if (_areasDirectory == null)
                return false;

            if (!entryPath.StartsWith("zone/filelist"))
                return false;

            // Slip an empty archives
            if (entryPath.EndsWith("2"))
                return false;

            string binaryName;
            switch (InteractionService.GamePart)
            {
                case FFXIIIGamePart.Part1:
                    binaryName = $"white_{entryPath.Substring(14, 5)}_img{(entryPath.EndsWith("2") ? "2" : string.Empty)}.win32.bin";
                    break;
                case FFXIIIGamePart.Part2:
                    binaryName = $"white_{entryPath.Substring(14, 6)}_img{(entryPath.EndsWith("2") ? "2" : string.Empty)}.win32.bin";
                    break;
                default:
                    throw new NotSupportedException("InteractionService.GamePart");
            }

            string binaryPath = Path.Combine(_areasDirectory, binaryName);
            if (!File.Exists(binaryPath))
                return false;

            ArchiveAccessor accessor = parentListing.Accessor.CreateDescriptor(binaryPath, entry);
            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(UiArchiveExtension.Bin);
            container.Add(new UiArchiveNode(accessor, parentListing));

            return true;
        }

        private bool TryAddMoviesListing(ArchiveListing parentListing, ArchiveEntry entry, String entryName)
        {
            switch (entryName)
            {
                case "movie_items.win32.wdb":
                case "movie_items_us.win32.wdb":
                    break;
                default:
                    return false;
            }

            UiArchiveExtension extension = GetArchiveExtension(entry);

            UiDataTableNode node = new UiDataTableNode(parentListing, extension, entry);
            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(extension);
            container.Add(node);

            return true;
        }

        private bool TryAddImgbPair(ArchiveListing listing, ArchiveEntry entry, string entryPath, string entryName)
        {
            string ext = PathEx.GetMultiDotComparableExtension(entryName);
            switch (ext)
            {
                case ".win32.xfv":
                case ".win32.xgr":
                case ".win32.xwb":
                    break;
                case ".win32.trb":
                    break;
                case ".win32.imgb":
                    break;
                default:
                    return false;
            }

            string longName = entryPath.Substring(0, entryPath.Length - ext.Length);
            if (IsUnexpectedEntry(listing.Name, longName))
                return false;

            return SetPairedEntry(listing, entry, ext, longName);
        }

        private bool SetPairedEntry(ArchiveListing listing, ArchiveEntry entry, string ext, string longName)
        {
            Pair<ArchiveEntry, ArchiveEntry> pair = ProvidePair(longName);

            if (ext == ".win32.imgb")
                pair.Item2 = entry;
            else
                pair.Item1 = entry;

            if (!pair.IsAnyEmpty)
            {
                UiArchiveExtension extension = GetArchiveExtension(pair.Item1);

                UiFileTableNode node = new UiFileTableNode(listing, extension, pair.Item1, pair.Item2);
                ConcurrentBag<UiNode> container = ProvideRootNodeChilds(extension);
                container.Add(node);
            }

            return true;
        }

        private UiArchiveExtension GetArchiveExtension(ArchiveEntry indices)
        {
            string ext = PathEx.GetMultiDotComparableExtension(indices.Name);

            const string extensionPrefix = ".win32.";
            ext = ext.Substring(extensionPrefix.Length);
            return EnumCache<UiArchiveExtension>.Parse(ext);
        }

        private bool IsUnexpectedEntry(string listingName, string longName)
        {
            if (InteractionService.GamePart != FFXIIIGamePart.Part1)
                return false;

            const string zoneFileListPrefix = @"zone/filelist_z";
            const string zoneBgLogPrefix = @"bg/loc";

            if (listingName.StartsWith(zoneFileListPrefix) && longName.StartsWith(zoneBgLogPrefix))
            {
                if (listingName.Substring(zoneFileListPrefix.Length, 3) != longName.Substring(zoneBgLogPrefix.Length, 3))
                    return true;
            }

            return false;
        }
    }
}