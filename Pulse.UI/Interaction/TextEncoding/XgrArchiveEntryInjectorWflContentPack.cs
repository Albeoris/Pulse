using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class XgrArchiveEntryInjectorWflContentPack : IXgrArchiveEntryInjector
    {
        private readonly WflContent _content;
        private readonly WpdEntry _targetEntry;

        public XgrArchiveEntryInjectorWflContentPack(WflContent content, WpdEntry targetEntry)
        {
            _content = content;
            _targetEntry = targetEntry;
        }

        public int CalcSize()
        {
            return _targetEntry.Length;
        }

        public void Inject(Stream indices, Stream content, Action<long> progress)
        {
            using (MemoryStream ms = new MemoryStream(1024))
            {
                WflFileWriter writer = new WflFileWriter(ms);
                writer.Write(_content);

                ms.Position = 0;
                XgrArchiveEntryInjectorPack.Inject(indices, _targetEntry, ms, (int)ms.Length, null);
            }

            progress.NullSafeInvoke(_targetEntry.Length);
        }
    }
}