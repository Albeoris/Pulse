using System;
using System.Globalization;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WflFileReader
    {
        private const int ColorTableMagic = 0x00016A28;

        private readonly Stream _input;
        private readonly BinaryReader _br;

        public WflFileReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
            _br = new BinaryReader(_input);
        }

        public WflContent Read()
        {
            WflHeader header = ReadHeader();

            short[] additionalTable = new short[WflContent.AdditionalTableCount];
            int[] sizes = new int[256 * 2];
            int[] offsets = new int[256 * 2];
            int sizesIndex = 0x20;
            int offsetIndex = 0x20;

            for (int i = 0; i < WflContent.CharactersCount; i++)
                sizes[sizesIndex++] = _br.ReadInt32();

            sizesIndex += 0x20;

            int unknownValue = _br.ReadInt32();
            if (unknownValue != ColorTableMagic)
                throw Exceptions.CreateException(Lang.Error.File.UnknownFormat);

            int colorsCount = 0;
            switch (header.ColorTableType)
            {
                case WflHeader.ColorTable40C:
                    colorsCount = 0x40C / 4;
                    break;
                case WflHeader.ColorTable460:
                    colorsCount = 0x460 / 4;
                    break;
                case WflHeader.ColorTableFF23:
                case WflHeader.ColorTable530:
                    colorsCount = 0x530 / 4;
                    break;
                default:
                    throw new NotSupportedException(header.ColorTableType.ToString(CultureInfo.InvariantCulture));
            }

            int[] colors = new int[colorsCount];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = _br.ReadInt32();

            if (header.TableType == WflHeader.LargeTable)
            {
                for (int i = 0; i < WflContent.CharactersCount; i++)
                    offsets[offsetIndex++] = _br.ReadInt32();
                offsetIndex += 0x20;

                for (int i = 0; i < WflContent.CharactersCount; i++)
                    sizes[sizesIndex++] = _br.ReadInt32();

                for (int i = 0; i < additionalTable.Length; i++)
                    additionalTable[i] = _br.ReadInt16();

                for (int i = 0; i < WflContent.CharactersCount; i++)
                    offsets[offsetIndex++] = _br.ReadInt32();
            }

            return new WflContent(header, sizes, offsets, colors, additionalTable);
        }

        private WflHeader ReadHeader()
        {
            return _input.ReadStruct<WflHeader>();
        }
    }
}