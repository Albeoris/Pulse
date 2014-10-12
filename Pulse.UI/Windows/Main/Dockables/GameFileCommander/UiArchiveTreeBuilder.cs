using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        private const string SystemFolder = @"white_data\sys";

        public static UiArchiveTreeViewItem Build(string gamePath)
        {
            UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(gamePath);
            return builder.Build();
        }

        private readonly string _gamePath;

        private UiArchiveTreeBuilder(string gamePath)
        {
            _gamePath = gamePath;
        }

        public UiArchiveTreeViewItem Build()
        {
            string root = Path.Combine(_gamePath, SystemFolder);
            string[] lists = Directory.GetFiles(root, "filelist*.bin");
            UiArchiveNode rootNode = new UiArchiveNode();
            foreach (string list in lists)
            {
                using (FileStream input = new FileStream(list, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    ArchiveListingReader reader = new ArchiveListingReader(input);
                    ArchiveListing listing = reader.Read();
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
                        try
                        {
                            parent.Childs.Add(name, new UiArchiveNode {Name = name, Entry = entry});
                        }
                        catch
                        {
                            Console.WriteLine(entry.Name);
                        }
                    }
                }
            }

            return rootNode.GetTreeViewItem();
        }
    }
}