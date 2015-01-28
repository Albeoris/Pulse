using System;
using System.Globalization;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class StringsZtrFormatter : IZtrFormatter
    {
        private static readonly Lazy<StringsZtrFormatter> LazyInstance = new Lazy<StringsZtrFormatter>();

        public static StringsZtrFormatter Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Write(StreamWriter sw, ZtrFileEntry entry, int index)
        {
            sw.WriteLine("\"{0}║{1}\" = \"{2}\";",
                index.ToString("D4", CultureInfo.InvariantCulture),
                entry.Key,
                entry.Value.Replace("\\", "\\\\").Replace("\"", "\\\""));
        }

        public ZtrFileEntry Read(StreamReader sr, out int index)
        {
            index = -1;
            ZtrFileEntry result = new ZtrFileEntry();

            StringBuilder sb = new StringBuilder(512);
            bool key = false;
            bool block = false;
            bool escape = false;
            int line = 0;
            while (true)
            {
                int value = sr.Read();
                if (value < 0)
                {
                    if (sb.Length == 0)
                        return null;

                    throw Exceptions.CreateException("Неожиданный конец потока.");
                }

                char ch = (char)value;
                switch (ch)
                {
                    case '║':
                    {
                        if (!block)
                            continue;

                        index = int.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                        sb.Clear();
                        key = true;
                        break;
                    }
                    case '\\':
                    {
                        if (!block)
                            continue;

                        if (escape)
                        {
                            sb.Append('\\');
                            escape = false;
                        }
                        else
                        {
                            escape = true;
                        }
                        break;
                    }
                    case '"':
                    {
                        if (escape)
                        {
                            sb.Append('"');
                            escape = false;
                        }
                        else
                        {
                            if (block)
                            {
                                if (key)
                                {
                                    result.Key = sb.ToString();
                                    key = false;
                                }
                                else
                                {
                                    result.Value = sb.ToString();
                                    return result;
                                }
                                block = false;
                                sb.Clear();
                            }
                            else
                            {
                                block = true;
                            }
                        }
                        break;
                    }
                    case '\r':
                    case '\n':
                    {
                        if (!block)
                            continue;

                        line++;
                        break;
                    }
                    default:
                    {
                        if (!block)
                            continue;

                        if (line > 0)
                        {
                            for (int i = 0; i < (line + 1) / 2; i++)
                                sb.Append(Environment.NewLine);
                            line = 0;
                        }

                        sb.Append(ch);
                        break;
                    }
                }
            }
        }
    }
}