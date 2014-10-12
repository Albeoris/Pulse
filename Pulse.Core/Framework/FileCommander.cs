using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public static class FileCommander
    {
        public static string EnsureDirectoryExists(params string[] paths)
        {
            string path = Path.Combine(paths);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return path;
        }
    }
}
