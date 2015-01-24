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

        public static void WriteBig(this BinaryWriter self, Int16 value)
        {
            self.Write(Endian.SwapInt16(value));
        }

        public static void WriteBig(this BinaryWriter self, UInt16 value)
        {
            self.Write(Endian.SwapUInt16(value));
        }

        public static void WriteBig(this BinaryWriter self, Int32 value)
        {
            self.Write(Endian.SwapInt32(value));
        }

        public static void WriteBig(this BinaryWriter self, UInt32 value)
        {
            self.Write(Endian.SwapUInt32(value));
        }
    }
}