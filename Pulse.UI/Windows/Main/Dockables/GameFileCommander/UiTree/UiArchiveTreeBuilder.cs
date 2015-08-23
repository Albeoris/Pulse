using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        private readonly GameLocationInfo _gameLocation;

        public UiArchiveTreeBuilder(GameLocationInfo gameLocation)
        {
            _gameLocation = gameLocation;
        }

        public UiArchives Build()
        {
            string[] lists = _gameLocation.EnumerateListingFiless().ToArray();
            ConcurrentBag<UiArchiveNode> nodes = new ConcurrentBag<UiArchiveNode>();

            Parallel.ForEach(lists, fileName =>
            {
                ArchiveAccessor accessor = new ArchiveAccessor(GetBinaryFilePath(fileName), fileName);
                nodes.Add(new UiArchiveNode(accessor, null));
            });

            return new UiArchives(nodes.OrderBy(n=>n.Name).ToArray());
        }

        private string GetBinaryFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            if (fileName.StartsWith("filelist_scr", System.StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_scr", "white_scr"));
            if (fileName.StartsWith("filelist_patch", System.StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_patch", "white_patch"));

            return Path.Combine(directory, fileName.Replace("filelist", "white_img"));
        }
    }
}