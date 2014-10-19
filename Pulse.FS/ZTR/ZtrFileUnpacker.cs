using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileUnpacker
    {
        private Stream _input;
        private BinaryReader _br;

        public ZtrFileUnpacker(Stream input)
        {
            _input = input;
            _br = new BinaryReader(_input);
        }

        public ZtrFileEntry[] Unpack()
        {
            if (_input.Length - _input.Position < 5)
                return new ZtrFileEntry[0];

            ZtrFileType type = (ZtrFileType)_br.ReadInt32();
            switch (type)
            {
                case ZtrFileType.BigEndianUncompressedPair:
                    return ExtractBigEndianUncompressedPair();
                case ZtrFileType.LittleEndianCompressedDictionary:
                    return ExtractLittleEndianCompressedDictionary();
                default:
                    return ExtractBigEndianUncompressedDictionary((int)type);
            }
        }

        private ZtrFileEntry[] ExtractBigEndianUncompressedDictionary(int count)
        {
            if (count < 0 || count > 10240)
                throw new ArgumentOutOfRangeException("count", count.ToString());

            ZtrFileEntry[] entries = new ZtrFileEntry[count];
            entries.InitializeElements();

            int[] offsets = new int[count * 2];
            for (int i = 0; i < count; i++)
                offsets[i] = _br.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                _input.SetPosition(offsets[i]);
                entries[i].Key = ZtrFileHelper.ReadNullTerminatedString(_input);
            }

            for (int i = 0; i < count; i++)
            {
                _input.SetPosition(offsets[i + count]);
                entries[i].Value = ZtrFileHelper.ReadNullTerminatedString(_input);
            }

            return entries;
        }

        private ZtrFileEntry[] ExtractBigEndianUncompressedPair()
        {
            ZtrFileEntry result = new ZtrFileEntry();;

            int keyOffset = _br.ReadInt32();
            int textOffset = _br.ReadInt32();

            _input.SetPosition(keyOffset);
            result.Key = ZtrFileHelper.ReadNullTerminatedString(_input);

            _input.SetPosition(textOffset);
            result.Value = ZtrFileHelper.ReadNullTerminatedString(_input);

            return new[] {result};
        }

        private ZtrFileEntry[] ExtractLittleEndianCompressedDictionary()
        {
            ZtrFileHeader header = new ZtrFileHeader();
            header.ReadFromStream(_input);

            ZtrFileEntry[] result = new ZtrFileEntry[header.Count];
            result.InitializeElements();

            ZtrFileKeysUnpacker keysUnpacker = new ZtrFileKeysUnpacker(_input, result);
            keysUnpacker.Unpack(header.KeysUnpackedSize);

            ZtrFileTextUnpacker textUnpacker = new ZtrFileTextUnpacker(_input, result, header.TextLinesTable);
            textUnpacker.Unpack(header.TextBlockTable[header.TextBlockTable.Length - 1]);

            return result;
        }
    }
}