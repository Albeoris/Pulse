using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class XgrArchiveEntryInjectorPack : IXgrArchiveEntryInjector
    {
        public static XgrArchiveEntryInjectorPack TryCreate(string sourceDir, WpdEntry targetEntry)
        {
            string sourcePath = Path.Combine(sourceDir, targetEntry.Name + '.' + targetEntry.Extension);

            if (!File.Exists(sourcePath))
            {
                Log.Warning("[XgrArchiveEntryInjectorPack]Файл не найден: {0}", sourcePath);
                return null;
            }

            return new XgrArchiveEntryInjectorPack(sourcePath, targetEntry);
        }

        private readonly string _sourcePath;
        private readonly Stream _source;

        private readonly int _sourceSize;
        private readonly WpdEntry _targetEntry;

        private XgrArchiveEntryInjectorPack(string sourcePath, WpdEntry targetEntry)
        {
            _sourcePath = sourcePath;
            _sourceSize = (int)FileEx.GetSize(_sourcePath);
            _targetEntry = targetEntry;
        }

        public XgrArchiveEntryInjectorPack(Stream source, WpdEntry targetEntry)
        {
            _source = source;
            _sourceSize = (int)source.Length;
            _targetEntry = targetEntry;
        }

        public int CalcSize()
        {
            return _sourceSize;
        }

        public void Inject(Stream indices, Stream content, Action<long> progress)
        {
            if (_source == null)
            {
                using (Stream input = File.OpenRead(_sourcePath))
                    Inject(indices, _targetEntry, input, _sourceSize, progress);
            }
            else
            {
                Inject(indices, _targetEntry, _source, _sourceSize, progress);
            }
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

            source.CopyTo(indices, sourceSize, buff, progress);
            targetEntry.Length = sourceSize;
        }
    }
}