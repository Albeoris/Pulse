using System.IO;
using Pulse.Text;

namespace Pulse.FS
{
    public sealed class ZtrFileHelper
    {
        public static string ReadNullTerminatedString(Stream input)
        {
            using (MemoryStream ms = new MemoryStream(4096))
            {
                int nc;
                while ((nc = input.ReadByte()) != -1)
                {
                    if (nc == 0)
                        break;

                    ms.WriteByte((byte)nc);
                }

                byte[] array = ms.ToArray();
                return FFXIIITextEncoding.Encoding.GetString(array, 0, array.Length);
            }
        }
    }
}