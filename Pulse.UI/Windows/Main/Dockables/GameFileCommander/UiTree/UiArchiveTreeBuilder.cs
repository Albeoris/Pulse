using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public UiArchiveTreeBuilder(GameLocationInfo gameLocation)
        {
            _gameLocation = gameLocation;
        }

        public UiArchives Build()
        {
            string[] lists = Directory.GetFiles(_gameLocation.SystemDirectory, "filelist*.bin");
            ConcurrentBag<UiArchiveNode> nodes = new ConcurrentBag<UiArchiveNode>();

            Parallel.ForEach(lists, fileName =>
            {
                ArchiveAccessor accessor = new ArchiveAccessor(GetBinaryFilePath(fileName), fileName);
                nodes.Add(new UiArchiveNode(accessor, null));
            });

            return new UiArchives(nodes.ToArray());
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