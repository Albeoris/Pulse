using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ArchiveEntryInjectorTempRhadamantsTxtToZtr : IArchiveEntryInjector
    {
        private readonly ArchiveEntry _targetEntry;
        private readonly Dictionary<string, string> _entries;
        private readonly FFXIIITextEncoding _encoding;

        public ArchiveEntryInjectorTempRhadamantsTxtToZtr(ArchiveEntry targetEntry, Dictionary<string, string> entries, FFXIIITextEncoding encoding)
        {
            _targetEntry = targetEntry;
            _entries = entries;
            _encoding = encoding;
        }

        public int CalcSize()
        {
            return (int)_targetEntry.UncompressedSize;
        }

        public void Inject(ArchiveAccessor archiveAccessor, bool? wantCompress, Action<long> progress)
        {
            bool compress = wantCompress ?? _targetEntry.IsCompressed;
            byte[] copyBuff = new byte[Math.Min(_targetEntry.UncompressedSize, 32 * 1024)];

            ZtrFileEntry[] targetEntries;
            using (Stream input = archiveAccessor.ExtractBinary(_targetEntry))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(input, _encoding);
                targetEntries = unpacker.Unpack();
            }

            ZtrFileEntry[] entries = MergeEntries(targetEntries);

            byte[] data;
            using (MemoryStream buff = new MemoryStream((int)(_targetEntry.UncompressedSize + 1024)))
            {
                ZtrFilePacker packer = new ZtrFilePacker(buff, _encoding);
                packer.Pack(entries);
                data = buff.ToArray();
            }

            int uncompressedSize = data.Length;
            int compressedSize = uncompressedSize;

            if (!compress)
            {
                using (Stream output = archiveAccessor.OpenOrAppendBinary(_targetEntry, uncompressedSize))
                    output.Write(data, 0, uncompressedSize);
            }
            else
            {
                using (SafeUnmanagedArray buff = new SafeUnmanagedArray(uncompressedSize + 256))
                using (UnmanagedMemoryStream buffStream = buff.OpenStream(FileAccess.ReadWrite))
                using (MemoryStream input = new MemoryStream(data))
                {
                    compressedSize = ZLibHelper.Compress(input, buffStream, uncompressedSize);
                    using (Stream output = archiveAccessor.OpenOrAppendBinary(_targetEntry, compressedSize))
                    {
                        buffStream.Position = 0;
                        buffStream.CopyTo(output, compressedSize, copyBuff);
                    }
                }
            }

            _targetEntry.Size = compressedSize;
            _targetEntry.UncompressedSize = uncompressedSize;

            progress.NullSafeInvoke(_targetEntry.UncompressedSize);
        }

        private ZtrFileEntry[] MergeEntries(ZtrFileEntry[] targetEntries)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(targetEntries.Length);

            StringBuilder sb = new StringBuilder(1024);
            foreach (ZtrFileEntry entry in targetEntries)
            {
                string oldText = entry.Value;
                string newText;

                if (string.IsNullOrEmpty(oldText) || !_entries.TryGetValue(entry.Key, out newText))
                {
                    dic.Add(entry.Key, entry.Value);
                    continue;
                }

                GetEndingTags(oldText, sb);
                string oldEnding = sb.ToString();
                sb.Clear();

                int endingLength = GetEndingTags(newText, sb);
                int newLength = newText.Length - endingLength;
                sb.Clear();

                // ¬осстановление старых хвостов и тегов новой строки
                bool cr = false;
                for (int i = 0; i < newLength; i++)
                {
                    char ch = newText[i];
                    switch (ch)
                    {
                        case '\n':
                        {
                            cr = false;
                            sb.Append(NewLineTag);
                            break;
                        }
                        case '\r':
                        {
                            cr = true;
                            break;
                        }
                        default:
                        {
                            if (cr)
                            {
                                sb.Append(NewLineTag);
                                cr = false;
                            }

                            sb.Append(ch);
                            break;
                        }
                    }
                }

                sb.Append(oldEnding);
                dic[entry.Key] = sb.ToString();
                sb.Clear();
            }

            ZtrFileEntry[] result = new ZtrFileEntry[targetEntries.Length];
            for (int index = 0; index < targetEntries.Length; index++)
            {
                ZtrFileEntry entry = targetEntries[index];
                result[index] = new ZtrFileEntry {Key = entry.Key, Value = dic[entry.Key]};
            }
            return result;
        }

        private static int GetEndingTags(string text, StringBuilder sb)
        {
            int index = text.Length - 1;
            if (index < 0)
                return 0;

            while (text[index] == '}')
            {
                index = text.LastIndexOf('{', index - 1) - 1;
                if (index < 0)
                    break;
            }

            if (index == text.Length - 1)
                return 0;

            char[] chars = index < 0 ? text.ToCharArray() : text.ToCharArray(index + 1, text.Length - index - 1);

            int offset = 0;
            int left = chars.Length;
            int result = left;

            FFXIIITextTag tag;
            while (left > 0 && (tag = FFXIIITextTag.TryRead(chars, ref offset, ref left)) != null)
            {
                switch (tag.Code)
                {
                    case FFXIIITextTagCode.End:
                    case FFXIIITextTagCode.Escape:
                    case FFXIIITextTagCode.Text:
                        sb.Append(tag);
                        break;
                    default:
                        result = left;
                        sb.Clear();
                        break;
                }
            }

            if (left != 0)
                Log.Warning("[ArchiveEntryInjectorStringsToZtr.GetEndingTags] Ќеверна€ управл€юща€ последовательность: {0}", text);

            return result;
        }

        private static readonly FFXIIITextTag NewLineTag = new FFXIIITextTag(FFXIIITextTagCode.Text, FFXIIITextTagText.NewLine);
    }
}