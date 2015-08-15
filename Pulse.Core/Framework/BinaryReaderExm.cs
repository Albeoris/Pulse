using System;
using System.IO;

namespace Pulse.Core
{
    public static class BinaryReaderExm
    {
        public static Guid ReadGuid(this BinaryReader self)
        {
            Exceptions.CheckArgumentNull(self, "self");

            byte[] buff = new byte[16];
            self.BaseStream.EnsureRead(buff, 0, buff.Length);
            return new Guid(buff);
        }

        public static short ReadBigInt16(this BinaryReader self)
        {
            byte[] buff = self.BaseStream.EnsureRead(2);
            unsafe
            {
                fixed (byte* b = &buff[0])
                    return Endian.ToBigInt16(b);
            }
        }

        public static ushort ReadBigUInt16(this BinaryReader self)
        {
            byte[] buff = self.BaseStream.EnsureRead(2);
            unsafe
            {
                fixed (byte* b = &buff[0])
                    return Endian.ToBigUInt16(b);
            }
        }

        public static int ReadBigInt32(this BinaryReader self)
        {
            byte[] buff = self.BaseStream.EnsureRead(4);
            unsafe
            {
                fixed (byte* b = &buff[0])
                    return Endian.ToBigInt32(b);
            }
        }

        public static uint ReadBigUInt32(this BinaryReader self)
        {
            byte[] buff = self.BaseStream.EnsureRead(4);
            unsafe
            {
                fixed (byte* b = &buff[0])
                    return Endian.ToBigUInt32(b);
            }
        }

        public static T Check<T>(this BinaryReader self, Func<BinaryReader, T> func, T expected)
        {
            T value = func(self);
            if (Equals(value, expected))
                return value;

            throw new InvalidDataException($"Неожиданное значение: [{value}], ожидается [{expected}].");
        }
    }
}