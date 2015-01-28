using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Pulse.Core;

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
                List<ZtrFileEntry> result = new List<ZtrFileEntry>(count);

                for (int i = 0; i < count && !sr.EndOfStream; i++)
                {
                    int index;
                    ZtrFileEntry entry = _formatter.Read(sr, out index);
                    if (entry == null)
                        continue;
                    
                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        Log.Warning("Неверная запись [Key: {0}, Value: {1}] в файле: {2}", entry.Key, entry.Value, name);
                        continue;
                    }
                    
                    result.Add(entry);
                }

                if (result.Count != count)
                    Log.Warning("Неверное количество строк в файле: {0} из {1}", result.Count, count);

                return result.ToArray();
            }
        }
    }
}