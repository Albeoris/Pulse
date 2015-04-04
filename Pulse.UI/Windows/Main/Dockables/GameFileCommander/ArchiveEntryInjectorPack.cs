//using System;
//using System.IO;
//using Pulse.Core;
//using Pulse.FS;

//namespace Pulse.UI
//{
//    public sealed class ArchiveEntryInjectorPack : IArchiveEntryInjector
//    {
//        public static ArchiveEntryInjectorPack TryCreate(string sourceDir, ArchiveEntry targetEntry)
//        {
//            string sourcePath = Path.Combine(sourceDir, targetEntry.Name);

//            if (!File.Exists(sourcePath))
//            {
//                Log.Warning("[ArchiveEntryInjectorPack]Файл не найден: {0}", sourcePath);
//                return null;
//            }

//            return new ArchiveEntryInjectorPack(sourcePath, targetEntry);
//        }

//        private readonly string _sourcePath;
//        private readonly int _sourceSize;
//        private readonly ArchiveEntry _targetEntry;

//        private ArchiveEntryInjectorPack(string sourcePath, ArchiveEntry targetEntry)
//        {
//            _sourcePath = sourcePath;
//            _sourceSize = (int)FileEx.GetSize(_sourcePath);
//            _targetEntry = targetEntry;
//        }

//        public int CalcSize()
//        {
//            return _sourceSize;
//        }

//        public void Inject(ArchiveAccessor archiveAccessor, bool? wantCompress, Action<long> progress)
//        {
//            using (Stream input = File.OpenRead(_sourcePath))
//                Inject(archiveAccessor, _targetEntry, input, _sourceSize, wantCompress, progress);
//        }

//        public static void Inject(ArchiveAccessor archiveAccessor, ArchiveEntry targetEntry, Stream source, int sourceSize, bool? wantCompress, Action<long> progress)
//        {
//            bool compress = wantCompress ?? targetEntry.IsCompressed;
//            if (sourceSize < 128)
//                compress = false;
            
//            byte[] copyBuff = new byte[Math.Min(sourceSize, 32 * 1024)];
//            int compressedSize = sourceSize;

//            if (!compress)
//            {
//                using (Stream output = archiveAccessor.OpenOrAppendBinary(targetEntry, compressedSize))
//                    source.CopyToStream(output, sourceSize, copyBuff, progress);
//            }
//            else
//            {
//                using (SafeUnmanagedArray buff = new SafeUnmanagedArray(sourceSize + 256))
//                using (UnmanagedMemoryStream buffStream = buff.OpenStream(FileAccess.ReadWrite))
//                {
//                    compressedSize = ZLibHelper.Compress(source, buffStream, sourceSize);
//                    if (compressedSize >= sourceSize)
//                    {
//                        compressedSize = sourceSize;
//                        using (Stream output = archiveAccessor.OpenOrAppendBinary(targetEntry, compressedSize))
//                            source.CopyToStream(output, sourceSize, copyBuff);
//                    }
//                    else
//                    {
//                        using (Stream output = archiveAccessor.OpenOrAppendBinary(targetEntry, compressedSize))
//                        {
//                            buffStream.Position = 0;
//                            buffStream.CopyToStream(output, compressedSize, copyBuff);
//                        }
//                    }
//                }
//            }

//            targetEntry.Size = compressedSize;
//            targetEntry.UncompressedSize = sourceSize;

//            progress.NullSafeInvoke(sourceSize);
//        }
//    }
//}