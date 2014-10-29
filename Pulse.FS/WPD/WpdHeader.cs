using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WpdHeader
    {
        public const int MagicNumber = 0x00445057; // WPD

        public int Magic = MagicNumber;
        public int Count;
        public int[] Offsets;
        public int[] Lengths;
        public string[] Names;
        public string[] Extensions;

        public static unsafe WpdHeader ReadFromStream(Stream input)
        {
            WpdHeader result = new WpdHeader();

            if (input.Length - input.Position < 16)
                return result;

            byte[] buff = input.EnsureRead(16);
            fixed (byte* b = &buff[0])
            {
                result.Magic = Endian.ToBigInt32(b + 0);
                result.Count = Endian.ToBigInt32(b + 4);
            }

            result.Offsets = new int[result.Count];
            result.Lengths = new int[result.Count];
            result.Names = new string[result.Count];
            result.Extensions = new string[result.Count];

            char[] text = new char[16];

            buff = input.EnsureRead(result.Count * 32);
            fixed (int* o = &result.Offsets[0])
            fixed (int* l = &result.Lengths[0])
            fixed (char* t = &text[0])
            fixed (byte* b = &buff[0])
            {
                for (int i = 0; i < result.Count; i++)
                {
                    int offset = i * 32;
                    result.Names[i] = new string((sbyte*)b + offset);
                    *(o + i) = Endian.ToBigInt32(b + offset + 16);
                    *(l + i) = Endian.ToBigInt32(b + offset + 20);
                    result.Extensions[i] = new string((sbyte*)b + offset + 24);
                }
            }

            return result;
        }
    }
}