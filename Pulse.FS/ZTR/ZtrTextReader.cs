using System.Globalization;
using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrTextReader
    {
        private readonly Stream _input;
        private readonly IZtrFormatter _formatter;

        public ZtrTextReader(Stream input, IZtrFormatter formatter)
        {
            _input = input;
            _formatter = formatter;
        }

        public ZtrFileEntry[] Read(out string name)
        {
            using (StreamReader sr = new StreamReader(_input, Encoding.UTF8, true, 4096, true))
            {
                name = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    name = name.Substring(2, name.Length - 4);

                string countStr = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    countStr = countStr.Substring(2, countStr.Length - 4);
                int count = int.Parse(countStr, CultureInfo.InvariantCulture);
                ZtrFileEntry[] result = new ZtrFileEntry[count];

                for (int i = 0; i < count; i++)
                {
                    int index;
                    ZtrFileEntry entry = _formatter.Read(sr, out index);
                    result[index] = entry;
                }

                return result;
            }
        }
    }
}