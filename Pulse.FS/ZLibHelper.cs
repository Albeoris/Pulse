using System;
using System.IO;
using Pulse.Core;
using zlib;

namespace Pulse.FS
{
    public static class ZLibHelper
    {
        public static byte[] Uncompress(Stream input, int uncompressedSize)
        {
            byte[] buff = new byte[uncompressedSize];
            ZInputStream reader = new ZInputStream(input);
            reader.EnsureRead(buff, 0, uncompressedSize);
            return buff;
        }

        public static void EnsureRead(this ZInputStream self, byte[] buff, int offset, int size)
        {
            Exceptions.CheckArgumentNull(self, "self");

            int readed;
            while (size > 0 && (readed = self.read(buff, offset, size)) != 0)
            {
                size -= readed;
                offset += readed;
            }

            if (size != 0)
                throw new Exception("Неожиданный конец потока.");
        }
    }
}