using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        public static Task<UiArchives> BuildAsync(GameLocationInfo gameLocation)
        {
            return Task.Run(() =>
            {
                UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(gameLocation);
                return builder.Build();
            });
        }

        private readonly GameLocationInfo _gameLocation;

        private UiArchiveTreeBuilder(GameLocationInfo gameLocation)
        {
            _gameLocation = gameLocation;
        }

        public UiArchives Build()
        {
            string[] lists = Directory.GetFiles(_gameLocation.SystemDirectory, "filelist*.bin");
            ConcurrentBag<UiArchiveBuilderNode> nodes = new ConcurrentBag<UiArchiveBuilderNode>();

            Parallel.ForEach(lists, fileName =>
            {
                ArchiveAccessor accessor = new ArchiveAccessor(GetBinaryFilePath(fileName), fileName);
                ArchiveListing[] listings = ArchiveListingReader.Read(_gameLocation.AreasDirectory, accessor);
                Dictionary<string, Pair<ArchiveEntry, ArchiveEntry>> xgr = new Dictionary<string, Pair<ArchiveEntry, ArchiveEntry>>(16);

                foreach (ArchiveListing listing in listings)
                {
                    UiArchiveBuilderNode rootNode = new UiArchiveBuilderNode(listing.Name, listing);
                    foreach (ArchiveEntry entry in listing)
                    {
                        UiArchiveBuilderNode parent = rootNode;
                        string[] path = entry.Name.ToLowerInvariant().Split(Path.AltDirectorySeparatorChar);
                        for (int i = 0; i < path.Length - 1; i++)
                        {
                            UiArchiveBuilderNode child;
                            if (!parent.Childs.TryGetValue(path[i], out child))
                            {
                                child = new UiArchiveBuilderNode(path[i]);
                                parent.Childs.Add(child.Name, child);
                            }
                            parent = child;
                        }

                        string name = path.Last();
                        if (!parent.Childs.ContainsKey(name)) // Несколько файлов оказались в одном архиве дважды
                            parent.Childs.Add(name, new UiArchiveBuilderNode(name, entry));

                        string ext = PathEx.GetMultiDotComparableExtension(name);
                        switch (ext)
                        {
                            case ".win32.xgr":
                            case ".win32.imgb":
                            {
                                name = name.Substring(0, name.Length - ext.Length);
                                Pair<ArchiveEntry, ArchiveEntry> pair;
                                if (!xgr.TryGetValue(name, out pair))
                                {
                                    pair = new Pair<ArchiveEntry, ArchiveEntry>();
                                    xgr.Add(name, pair);
                                }

                                if (ext == ".win32.xgr")
                                    pair.Item1 = entry;
                                else
                                    pair.Item2 = entry;

                                if (!pair.IsAnyEmpty)
                                {
                                    XgrArchiveAccessor xgrAccessor = new XgrArchiveAccessor(listing.Accessor, pair.Item1, pair.Item2);
                                    XgrArchiveListing xgrListing = XgrArchiveListingReader.Read(xgrAccessor);
                                    xgrListing.ParentArchiveListing = listing;
                                    if (xgrListing.Count > 0)
                                    {
                                        UiArchiveBuilderNode xgrRoot = new UiArchiveBuilderNode(name + ".win32.unpack", xgrListing);
                                        foreach (WpdEntry xgrEntry in xgrListing)
                                            xgrRoot.Childs.Add(xgrEntry.Name, new UiArchiveBuilderNode(xgrEntry.Name + '.' + xgrEntry.Extension, xgrEntry));
                                        parent.Childs.Add(xgrRoot.Name, xgrRoot);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    nodes.Add(rootNode);
                }
            });

            return new UiArchives(nodes.OrderBy(n => n.Name).Select(n => n.Commit(null)).ToArray());
        }

        private string GetBinaryFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            if (fileName.StartsWith("filelist_scr", System.StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_scr", "white_scr"));

            return Path.Combine(directory, fileName.Replace("filelist", "white_img"));
        }
    }
}