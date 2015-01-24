using System;
using System.IO;

namespace Pulse.FS
{
    public interface IXgrArchiveEntryInjector
    {
        int CalcSize();
        void Inject(Stream indices, Stream content, Action<long> progress);
    }
}