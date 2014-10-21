using System.Globalization;
using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrTextReader
    {
        private readonly Stream _input;

        public ZtrTextReader(Stream input)
        {
            _input = input;
        }

        public ZtrFileEntry[] Read(out string name)
        {
            using (StreamReader sr = new StreamReader(_input, Encoding.UTF8, true, 4096, true))
            {
                name = sr.ReadLine();
                int count = int.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);
                ZtrFileEntry[] result = new ZtrFileEntry[count];

                for (int i = 0; i < count; i++)
                {
                    ZtrFileEntry entry = new ZtrFileEntry();
                    string[] line = sr.ReadLine().Split('║');
                    entry.Key = line[1];
                    entry.Value = line[2];
                    result[int.Parse(line[0], CultureInfo.InvariantCulture)] = entry;
                }

                return result;
            }
        }
    }
}