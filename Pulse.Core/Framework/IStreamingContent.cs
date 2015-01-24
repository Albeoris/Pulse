using System.IO;

namespace Pulse.Core
{
    public interface IStreamingContent
    {
        void ReadFromStream(Stream stream);
        void WriteToStream(Stream stream);
    }
}