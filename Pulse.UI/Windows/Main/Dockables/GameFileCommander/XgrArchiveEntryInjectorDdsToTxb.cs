using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;

namespace Pulse.UI
{
    public sealed class XgrArchiveEntryInjectorDdsToTxb : IXgrArchiveEntryInjector
    {
        public static XgrArchiveEntryInjectorDdsToTxb TryCreate(string sourceDir, WpdEntry targetEntry)
        {
            string sourcePath = Path.Combine(sourceDir, targetEntry.Name + ".dds");

            if (!File.Exists(sourcePath))
            {
                Log.Warning("[XgrArchiveEntryInjectorDdsToTxb]Файл не найден: {0}", sourcePath);
                return null;
            }

            return new XgrArchiveEntryInjectorDdsToTxb(sourcePath, targetEntry);
        }

        private readonly string _sourcePath;
        private readonly Stream _source;
        
        private readonly int _sourceSize;
        private readonly WpdEntry _targetEntry;

        private XgrArchiveEntryInjectorDdsToTxb(string sourcePath, WpdEntry targetEntry)
        {
            _sourcePath = sourcePath;
            _sourceSize = (int)FileEx.GetSize(_sourcePath);
            _targetEntry = targetEntry;
        }

        public XgrArchiveEntryInjectorDdsToTxb(Stream source, WpdEntry targetEntry)
        {
            _source = source;
            _sourceSize = (int)_source.Length;
            _targetEntry = targetEntry;
        }

        public int CalcSize()
        {
            return _sourceSize - 128;
        }

        public void Inject(Stream indices, Stream content, Action<long> progress)
        {
            if (_source != null)
            {
                Inject(indices, content, _targetEntry, _source, _sourceSize, progress);
            }
            else
            {
                using (Stream input = File.OpenRead(_sourcePath))
                    Inject(indices, content, _targetEntry, input, _sourceSize, progress);
            }
        }

        public static void Inject(Stream indices, Stream content, WpdEntry targetEntry, Stream source, int sourceSize, Action<long> progress)
        {
            indices.SetReadPosition(targetEntry.Offset);
            TextureSection textureHeader = indices.ReadContent<TextureSection>();
            GtexData data = textureHeader.Gtex;
            if (data.MipMapData.Length != 1)
                throw new NotImplementedException();

            DdsHeader ddsHeader = DdsHeaderDecoder.FromFileStream(source);
            DdsHeaderEncoder.ToGtexHeader(ddsHeader, data.Header);

            GtexMipMapLocation mipMapLocation = data.MipMapData[0];
            int dataSize = sourceSize - 128;
            if (dataSize <= mipMapLocation.Length)
            {
                content.Seek(mipMapLocation.Offset, SeekOrigin.Begin);
            }
            else
            {
                content.Seek(0, SeekOrigin.End);
                mipMapLocation.Offset = (int)content.Position;
            }

            byte[] buff = new byte[32 * 1024];
            source.CopyTo(content, dataSize, buff, progress);
            mipMapLocation.Length = dataSize;

            using (MemoryStream ms = new MemoryStream(96))
            {
                textureHeader.WriteToStream(ms);
                ms.SetPosition(0);

                XgrArchiveEntryInjectorPack.Inject(indices, targetEntry, ms, (int)ms.Length, null);
            }
        }
    }
}