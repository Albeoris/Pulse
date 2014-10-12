using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        private const string SystemFolder = @"white_data\sys";

        public static UiArchiveTreeViewItem[] Build(string gamePath)
        {
            UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(gamePath);
            return builder.Build();
        }

        private readonly string _gamePath;

        private UiArchiveTreeBuilder(string gamePath)
        {
            _gamePath = gamePath;
        }

        public UiArchiveTreeViewItem[] Build() 
        {
            string root = Path.Combine(_gamePath, SystemFolder);
            string[] lists = Directory.GetFiles(root, "filelist*.bin");
            ConcurrentBag<UiArchiveNode> nodes = new ConcurrentBag<UiArchiveNode>();
            Parallel.ForEach(lists, fileName =>
            {
                UiArchiveNode rootNode = new UiArchiveNode {Name = Path.GetFileName(fileName)};
                using (FileStream input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    ArchiveListingReader reader = new ArchiveListingReader(input);
                    ArchiveListing listing = reader.Read();
                    listing.ListingFile = fileName;
                    listing.BinaryFile = GetBinaryFilePath(fileName);
                    rootNode.Entry = listing;
                    foreach (ArchiveListingEntry entry in listing)
                    {
                        UiArchiveNode parent = rootNode;
                        string[] path = entry.Name.ToLowerInvariant().Split(Path.AltDirectorySeparatorChar);
                        for (int i = 0; i < path.Length - 1; i++)
                        {
                            UiArchiveNode child;
                            if (!parent.Childs.TryGetValue(path[i], out child))
                            {
                                child = new UiArchiveNode {Name = path[i]};
                                parent.Childs.Add(child.Name, child);
                            }
                            parent = child;
                        }

                        string name = path.Last();
                        if (!parent.Childs.ContainsKey(name)) // Несколько файлов оказались в одном архиве дважды
                            parent.Childs.Add(name, new UiArchiveNode {Name = name, Entry = entry});
                    }
                }
                nodes.Add(rootNode);
            });

            return nodes.Select(n => n.GetTreeViewItem()).ToArray();
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