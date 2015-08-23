using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public abstract class SectionBase : IStreamingContent
    {
        public SectionHeader SectionHeader;

        public virtual void ReadFromStream(Stream stream)
        {
            SectionHeader = stream.ReadContent<SectionHeader>();
        }

        public virtual void WriteToStream(Stream stream)
        {
            stream.WriteContent(SectionHeader);
        }
    }
}