using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WflFileWriter
    {
        private const int ColorTableMagic = 0x00016A28;

        private readonly Stream _output;
        private readonly BinaryWriter _bw;

        public WflFileWriter(Stream output)
        {
            _output = Exceptions.CheckArgumentNull(output, "output");
            _bw = new BinaryWriter(_output);
        }

        public void Write(WflContent content)
        {
            WriteHeader(content.Header);

            int sizesIndex = 0x20;
            int offsetIndex = 0x20;

            for (int i = 0; i < WflContent.CharactersCount; i++)
                _bw.Write(content.Sizes[sizesIndex++]);

            _bw.Write(ColorTableMagic);

            for (int i = 0; i < content.Colors.Length; i++)
                _bw.Write(content.Colors[i]);

            if (content.Header.TableType == WflHeader.LargeTable)
            {
                for (int i = 0; i < WflContent.CharactersCount; i++)
                    _bw.Write(content.Offsets[offsetIndex++]);

                sizesIndex += 0x20;
                offsetIndex += 0x20;

                for (int i = 0; i < WflContent.CharactersCount; i++)
                    _bw.Write(content.Sizes[sizesIndex++]);

                for (int i = 0; i < content.AdditionalTable.Length; i++)
                    _bw.Write(content.AdditionalTable[i]);

                for (int i = 0; i < WflContent.CharactersCount; i++)
                    _bw.Write(content.Offsets[offsetIndex++]);
            }
        }

        private void WriteHeader(WflHeader header)
        {
            _output.WriteStruct(header);
        }
    }
}