using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileUnpacker
    {
        private Stream _input;
        private Stream _output;
        private BinaryReader _br;

        public ZtrFileUnpacker(Stream input, Stream output)
        {
            _input = input;
            _output = output;
            _br = new BinaryReader(_input);
        }

        public void Unpack()
        {
            ZtrFileType type = (ZtrFileType)_br.ReadInt32();
            switch (type)
            {
                case ZtrFileType.BigEndianUncompressedPair:
                    ExtractBigEndianUncompressedPair();
                    break;
                case ZtrFileType.LittleEndianCompressedDictionary:
                    ExtractLittleEndianCompressedDictionary();
                    break;
                default:
                    ExtractBigEndianUncompressedDictionary((int)type);
                    break;
            }
        }

        private void ExtractBigEndianUncompressedDictionary(int count)
        {
            if (count < 0 || count > 2048)
                throw new ArgumentOutOfRangeException("count", count.ToString());

            count *= 2;

            int[] offsets = new int[count];
            for (int i = 0; i < count; i++)
                offsets[i] = _br.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                _input.SetPosition(offsets[i]);
                //key
                while (!_input.IsEndOfStream())
                {
                    int value = _input.ReadByte();
                    _output.WriteByte((byte)value);
                    if (value == 0)
                        break;
                }
            }
        }

        private void ExtractBigEndianUncompressedPair()
        {
            int keyLength = _br.ReadInt32();
            int textOffset = _br.ReadInt32();

            byte[] buff = _input.EnsureRead(keyLength);
            _output.Write(buff, 0, buff.Length);

            if (_input.Position != textOffset)
                throw new NotImplementedException();

            _input.CopyTo(_output);
        }

        private void ExtractLittleEndianCompressedDictionary()
        {
            ZtrFileHeader header = new ZtrFileHeader();
            header.ReadFromStream(_input);

            ZtrFileTagsUnpacker tagsUnpacker = new ZtrFileTagsUnpacker(_input, _output);
            tagsUnpacker.Unpack(header.TagsUnpackedSize);

            ZtrFileTextUnpacker textUnpacker = new ZtrFileTextUnpacker(_input, _output);
            textUnpacker.Unpack(header.TextBlockTable[header.TextBlockTable.Length - 1]);
        }
    }
}