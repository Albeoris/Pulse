using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileHelper
    {
        public static string ReadNullTerminatedString(Stream input, Encoding encoding, int zeroCount)
        {
            using (MemoryStream ms = new MemoryStream(4096))
            {
                int nc, count = 0;
                while ((nc = input.ReadByte()) != -1)
                {
                    if (nc == 0)
                    {
                        if (++count == zeroCount)
                        {
                            count = 0;
                            break;
                        }

                        continue;
                    }

                    while (count > 0)
                    {
                        count--;
                        ms.WriteByte(0);
                    }

                    ms.WriteByte((byte)nc);
                }

                while (count > 0)
                {
                    count--;
                    ms.WriteByte(0);
                }

                byte[] array = ms.ToArray();
                return encoding.GetString(array, 0, array.Length);
            }
        }
    }
}