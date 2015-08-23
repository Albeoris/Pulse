using System.Runtime.CompilerServices;

namespace Pulse.Core
{
    public static class MathEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte RoundUp(sbyte value, sbyte fold)
        {
            return checked((sbyte)(((value / fold) + 1) * fold));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short RoundUp(short value, short fold)
        {
            return checked((short)(((value / fold) + 1) * fold));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundUp(int value, int fold)
        {
            return ((value / fold) + 1) * fold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RoundUp(long value, long fold)
        {
            return ((value / fold) + 1) * fold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RoundUp(byte value, byte fold)
        {
            return checked((byte)(((value / fold) + 1) * fold));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RoundUp(ushort value, ushort fold)
        {
            return checked((ushort)(((value / fold) + 1) * fold));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundUp(uint value, uint fold)
        {
            return ((value / fold) + 1) * fold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RoundUp(ulong value, ulong fold)
        {
            return ((value / fold) + 1) * fold;
        }
    }
}