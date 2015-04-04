using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    // Obsolete
    public sealed class XgrArchiveEntryInjectorWflContentPack
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
                Inject(indices, _targetEntry, ms, (int)ms.Length, null);
            }

            progress.NullSafeInvoke(_targetEntry.Length);
        }

        public static void Inject(Stream indices, WpdEntry targetEntry, Stream source, int sourceSize, Action<long> progress)
        {
            byte[] buff = new byte[Math.Min(sourceSize, 32 * 1024)];

            if (sourceSize <= targetEntry.Length)
            {
                indices.Seek(targetEntry.Offset, SeekOrigin.Begin);
            }
            else
            {
                indices.Seek(0, SeekOrigin.End);
                targetEntry.Offset = (int)indices.Position;
            }

            source.CopyToStream(indices, sourceSize, buff, progress);
            targetEntry.Length = sourceSize;
        }
    }
}