using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public abstract class YkdResourceViewport : IStreamingContent
    {
        public abstract YkdResourceViewportType Type { get; }
        public abstract int Size { get; }

        public abstract void ReadFromStream(Stream stream);
        public abstract void WriteToStream(Stream stream);
        public abstract YkdResourceViewport Clone();

        public static YkdResourceViewport ReadFromStream(YkdResourceViewportType type, Stream stream)
        {
            YkdResourceViewport result;

            switch (type)
            {
                case YkdResourceViewportType.Empty:
                    result = new EmptyYkdResourceViewport();
                    break;
                case YkdResourceViewportType.Full:
                    result = new FullYkdResourceViewport();
                    break;
                case YkdResourceViewportType.Fragment:
                    result = new FragmentYkdResourceViewport();
                    break;
                case YkdResourceViewportType.Extra:
                    result = new ExtraYkdResourceViewport();
                    break;
                default:
                    throw new NotImplementedException(type.ToString());
            }

            result.ReadFromStream(stream);
            return result;
        }
    }
}