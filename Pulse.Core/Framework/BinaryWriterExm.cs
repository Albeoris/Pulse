using System;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;

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

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteBig(byte[] array, int offset, Int32 value)
        {
            fixed (byte* numPtr = &array[offset])
                WriteBig(numPtr, value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteBig(byte* numPtr, Int32 value)
        {
            *(int*)numPtr = Endian.SwapInt32(value);
        }
    }
}