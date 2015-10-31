using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileUnpacker
    {
        private readonly Stream _input;
        private readonly BinaryReader _br;
        private readonly FFXIIITextEncoding _encoding;

        public ZtrFileType Type { get; private set; }

        public ZtrFileUnpacker(Stream input, FFXIIITextEncoding encoding)
        {
            _encoding = encoding;
            _input = input;
            _br = new BinaryReader(_input);
        }

        public ZtrFileEntry[] Unpack()
        {
            if (_input.Length < 5)
                return new ZtrFileEntry[0];

            Type = (ZtrFileType)_br.ReadInt32();
            switch (Type)
            {
                case ZtrFileType.LittleEndianUncompressedPair:
                    return ExtractLittleEndianUncompressedPair();
                case ZtrFileType.BigEndianCompressedDictionary:
                    return ExtractBigEndianCompressedDictionary();
                default:
                    return ExtractLittleEndianUncompressedDictionary((int)Type);
            }
        }

        private ZtrFileEntry[] ExtractLittleEndianUncompressedDictionary(int count)
        {
            if (count < 0 || count > 102400)
                throw new ArgumentOutOfRangeException(nameof(count), count.ToString());

            ZtrFileEntry[] entries = new ZtrFileEntry[count];
            entries.InitializeElements();

            int[] offsets = new int[count * 2];
            for (int i = 0; i < count * 2; i++)
                offsets[i] = _br.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                _input.Position = offsets[i * 2];
                entries[i].Key = _input.ReadNullTerminatedString(Encoding.ASCII, 1);
                _input.Position = offsets[i * 2 + 1];
                entries[i].Value = _input.ReadNullTerminatedString(_encoding, 2);
            }

            return entries;
        }

        private ZtrFileEntry[] ExtractLittleEndianUncompressedPair()
        {
            ZtrFileEntry result = new ZtrFileEntry();

            int keyOffset = _br.ReadInt32();
            int textOffset = _br.ReadInt32();

            _input.Position = keyOffset;
            result.Key = _input.ReadNullTerminatedString(Encoding.ASCII, 1);

            _input.Position = textOffset;
            result.Value = _input.ReadNullTerminatedString(_encoding, 2);

            return new[] {result};
        }

        private ZtrFileEntry[] ExtractBigEndianCompressedDictionary()
        {
            ZtrFileHeader header = new ZtrFileHeader();
            header.ReadFromStream(_input);

            ZtrFileEntry[] result = new ZtrFileEntry[header.Count];
            result.InitializeElements();

            ZtrFileKeysUnpacker keysUnpacker = new ZtrFileKeysUnpacker(_input, result);
            keysUnpacker.Unpack(header.KeysUnpackedSize);

            ZtrFileTextUnpacker textUnpacker = new ZtrFileTextUnpacker(_input, result, header.TextLinesTable, _encoding);
            textUnpacker.Unpack(header.TextBlockTable[header.TextBlockTable.Length - 1]);

            return result;
        }
    }
}