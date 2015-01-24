using System.Globalization;
using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrTextWriter
    {
        private readonly Stream _output;
        private readonly IZtrFormatter _formatter;

        public ZtrTextWriter(Stream output, IZtrFormatter formatter)
        {
            _output = output;
            _formatter = formatter;
        }

        public void Write(string name, ZtrFileEntry[] entries)
        {
            using (StreamWriter sw = new StreamWriter(_output, Encoding.UTF8, 4096, true))
            {
                if (_formatter is StringsZtrFormatter) // TEMP
                {
                    sw.WriteLine("/*" + name + "*/");
                    sw.WriteLine("/*" + entries.Length.ToString("D4", CultureInfo.InvariantCulture) + "*/");
                }
                else
                {
                    sw.WriteLine(name);
                    sw.WriteLine(entries.Length.ToString("D4", CultureInfo.InvariantCulture));
                }

                for (int i = 0; i < entries.Length; i++)
                    _formatter.Write(sw, entries[i], i);
            }
        }
    }
}