using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public class TextureSection : SectionBase
    {
        public TextureHeader TextureHeader;
        public GtexData Gtex;

        public override void ReadFromStream(Stream input)
        {
            base.ReadFromStream(input);
            TextureHeader = input.ReadContent<TextureHeader>();
            Gtex = input.ReadContent<GtexData>();
        }

        public override void WriteToStream(Stream output)
        {
            base.WriteToStream(output);
            output.WriteContent(TextureHeader);
            output.WriteContent(Gtex);
        }
    }
}