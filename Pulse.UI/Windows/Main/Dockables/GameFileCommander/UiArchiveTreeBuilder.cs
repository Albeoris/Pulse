using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        public static Task<UiArchiveNode[]> BuildAsync(GameLocationInfo gameLocation)
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

        public UiArchiveNode[] Build()
        {
            string[] lists = Directory.GetFiles(_gameLocation.SystemDirectory, "filelist*.bin");
            ConcurrentBag<UiArchiveBuilderNode> nodes = new ConcurrentBag<UiArchiveBuilderNode>();

            Parallel.ForEach(lists, fileName =>
            {
                ArchiveAccessor accessor = new ArchiveAccessor(GetBinaryFilePath(fileName), fileName);
                ArchiveListing[] listings = ArchiveListingReader.Read(_gameLocation.AreasDirectory, accessor);

                foreach (ArchiveListing listing in listings)
                {
                    UiArchiveBuilderNode rootNode = new UiArchiveBuilderNode(listing.Name, listing);
                    foreach (ArchiveListingEntry entry in listing)
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
                    }
                    nodes.Add(rootNode);
                }
            });

            return nodes.OrderBy(n => n.Name).Select(n => n.Commit(null)).ToArray();
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