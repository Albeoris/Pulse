using System;
using System.IO;
using Pulse.Text;

namespace Pulse.FS
{
    public sealed class ZtrFilePacker
    {
        private Stream _output;
        private BinaryWriter _bw;

        public ZtrFilePacker(Stream output)
        {
            _output = output;
            _bw = new BinaryWriter(_output);
        }

        public void Pack(ZtrFileEntry[] entries)
        {
            if (entries.Length == 0)
                PackLittleEndianCompressedDictionary(entries);
            else if (entries.Length == 1)
                PackBigEndianUncompressedPair(entries[0]);
            else
                PackBigEndianUncompressedDictionary(entries);
        }

        private void PackBigEndianUncompressedDictionary(ZtrFileEntry[] entries)
        {
            int count = entries.Length;
            byte[][] keys = new byte[count][];
            byte[][] values = new byte[count][];
            int[] offsets = new int[count * 2];
            for (int i = 0; i < count; i++)
            {
                ZtrFileEntry entry = entries[i];
                keys[i] = FFXIIITextEncoding.Encoding.GetBytes(entry.Key);
                values[i] = FFXIIITextEncoding.Encoding.GetBytes(entry.Value);
                offsets[i + 1] = offsets[i] + keys.Length + 1;
            }
            for (int i = 0; i < count; i++)
            {
                if (i < count - 1)
                    offsets[count + i + 1] = offsets[count + i] + values[i].Length + 2;
            }
            _bw.Write(count);
            for (int i = 0; i < count * 2; i++)
                _bw.Write(4 + count * 2 * 4 + offsets[i]);
            for (int i = 0; i < count; i++)
            {
                _bw.Write(keys[i], 0, keys[i].Length);
                _bw.Write((byte)0);
            }
            for (int i = 0; i < count; i++)
            {
                _bw.Write(values[i], 0, values[i].Length);
                _bw.Write((short)0);
            }
        }

        private void PackBigEndianUncompressedPair(ZtrFileEntry entry)
        {
            _bw.Write((int)ZtrFileType.BigEndianUncompressedPair);

            byte[] key = FFXIIITextEncoding.Encoding.GetBytes(entry.Key);
            byte[] value = FFXIIITextEncoding.Encoding.GetBytes(entry.Value);

            _bw.Write(4);
            _bw.Write(4 + key.Length + 1);

            _bw.Write(key);
            _bw.Write((byte)0);
            _bw.Write(value);
            _bw.Write((short)0);
        }

        private void PackLittleEndianCompressedDictionary(ZtrFileEntry[] entries)
        {
            if (entries.Length > 0)
                throw new NotImplementedException();

            _bw.Write((int)ZtrFileType.LittleEndianCompressedDictionary);
        }
    }
}