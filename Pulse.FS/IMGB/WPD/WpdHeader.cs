using System;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public class WpdHeader : IStreamingContent
    {
        public const int MagicNumber = 0x00445057; // WPD

        public int Magic = MagicNumber;
        public int Count;
        public WpdEntry[] Entries;

        public virtual unsafe void ReadFromStream(Stream input)
        {
            if (input.Length - input.Position < 16)
                return;

            byte[] buff = input.EnsureRead(16);
            fixed (byte* b = &buff[0])
            {
                Magic = Endian.ToBigInt32(b + 0);
                Count = Endian.ToBigInt32(b + 4);
            }

            Entries = new WpdEntry[Count];
            if (Count < 1)
                return;

            buff = input.EnsureRead(Count * 32);
            fixed (byte* b = &buff[0])
            {
                for (int i = 0; i < Count; i++)
                {
                    int offset = i * 32;

                    Entries[i] = new WpdEntry(
                        i,
                        new string((sbyte*)b + offset),
                        Endian.ToBigInt32(b + offset + 16),
                        Endian.ToBigInt32(b + offset + 20),
                        new string((sbyte*)b + offset + 24));
                }
            }
        }

        public virtual void WriteToStream(Stream stream)
        {
            if (Entries == null)
                return;

            Count = Entries.Length;

            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(MagicNumber);
            bw.WriteBig(Count);
            bw.Write(0L);

            if (Count < 1)
                return;

            for (int i = 0; i < Count; i++)
            {
                WpdEntry entry = Entries[i];

                byte[] bytes = Encoding.ASCII.GetBytes(entry.NameWithoutExtension);
                Array.Resize(ref bytes, 16);
                bw.Write(bytes, 0, bytes.Length);

                bw.WriteBig(entry.Offset);
                bw.WriteBig(entry.Length);

                bytes = Encoding.ASCII.GetBytes(entry.Extension);
                Array.Resize(ref bytes, 8);
                bw.Write(bytes, 0, bytes.Length);
            }
        }
    }
}