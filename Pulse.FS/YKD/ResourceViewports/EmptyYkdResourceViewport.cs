using System.IO;

namespace Pulse.FS
{
    public sealed class EmptyYkdResourceViewport : YkdResourceViewport
    {
        public override YkdResourceViewportType Type
        {
            get { return YkdResourceViewportType.Empty; }
        }

        public override int Size
        {
            get { return 0; }
        }

        public override void ReadFromStream(Stream stream)
        {
        }

        public override void WriteToStream(Stream stream)
        {
        }

        public override YkdResourceViewport Clone()
        {
            return new EmptyYkdResourceViewport();
        }
    }
}