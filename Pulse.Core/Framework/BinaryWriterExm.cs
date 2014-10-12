using System;
using System.IO;

namespace Pulse.Core
{
    public static class BinaryWriterExm
    {
        public static void Write(this BinaryWriter self, Guid value)
        {
            Exceptions.CheckArgumentNull(self, "self");

            byte[] buff = value.ToByteArray();
            self.Write(buff, 0, buff.Length);
        }
    }
}