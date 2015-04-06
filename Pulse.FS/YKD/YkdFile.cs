using System.IO;
using System.Runtime;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdFile : IStreamingContent
    {
        public static readonly Encoding NamesEncoding = Encoding.GetEncoding(1252);

        public YkdHeader Header;
        public YkdBlock Background;
        public YkdOffsets Offsets;
        public YkdBlock[] Blocks;
        public YkdResources Resources;

        public void ReadFromStream(Stream stream)
        {
            Header = stream.ReadContent<YkdHeader>();
            Background = stream.ReadContent<YkdBlock>();
            Offsets = stream.ReadContent<YkdOffsets>();
            Blocks = new YkdBlock[Offsets.Count];
            for (int i = 0; i < Blocks.Length; i++)
            {
                stream.SetPosition(Offsets[i]);
                Blocks[i] = stream.ReadContent<YkdBlock>();
            }
            Resources = stream.ReadContent<YkdResources>();
        }

        public void WriteToStream(Stream stream)
        {
            stream.WriteContent(Header);
            stream.WriteContent(Background);
            YkdOffsets.WriteToStream(stream, ref Offsets, ref Blocks, b => b.CalcSize());
            stream.WriteContent(Blocks);
            stream.WriteContent(Resources);
        }
    }
}