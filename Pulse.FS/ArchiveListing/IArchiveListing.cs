using System.Collections;

namespace Pulse.FS
{
    public interface IArchiveListing : IList
    {
        string Name { get; }
    }
}