using System.Globalization;
using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrTextWriter
    {
        private readonly Stream _output;

        public ZtrTextWriter(Stream output)
        {
            _output = output;
        }

        public void Write(string name, ZtrFileEntry[] entries)
        {
            using (StreamWriter sw = new StreamWriter(_output, Encoding.UTF8, 4096, true))
            {
                sw.WriteLine(name);
                sw.WriteLine(entries.Length.ToString("D4", CultureInfo.InvariantCulture));
                for (int i = 0; i < entries.Length; i++)
                {
                    ZtrFileEntry entry = entries[i];
                    sw.WriteLine("{0}║{1}║{2}", i.ToString("D4", CultureInfo.InvariantCulture), entry.Key, entry.Value);
                }
            }
        }
    }
}
