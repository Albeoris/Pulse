using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class StringsToZtrArchiveEntryInjector : IArchiveEntryInjector
    {
        public string SourceExtension
        {
            get { return ".strings"; }
        }

        public bool TryInject(IUiInjectionSource source, string sourceFullPath, ArchiveEntryInjectionData data, ArchiveEntry entry)
        {
            ZtrFileEntry[] sourceEntries;
            using (Stream input = source.TryOpen(sourceFullPath))
            {
                if (input != null)
                {
                    string entryName;
                    ZtrTextReader reader = new ZtrTextReader(input, StringsZtrFormatter.Instance);
                    sourceEntries = reader.Read(out entryName);
                    using (Stream output = data.OuputStreamFactory(entry))
                        Inject(data.Listing, entry, sourceEntries, output);

                    return true;
                }
            }

            MemoryInjectionSource memorySource = source as MemoryInjectionSource;
            if (memorySource == null)
                return false;

            sourceEntries = memorySource.TryProvideStrings();
            if (sourceEntries == null)
                return false;

            using (Stream output = data.OuputStreamFactory(entry))
                Inject(data.Listing, entry, sourceEntries, output);

            return true;
        }

        private void Inject(ArchiveListing listing, ArchiveEntry entry, ZtrFileEntry[] sourceEntries, Stream output)
        {
            ZtrFileEntry[] targetEntries;
            using (Stream original = listing.Accessor.ExtractBinary(entry))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(original, InteractionService.TextEncoding.Provide().Encoding);
                targetEntries = unpacker.Unpack();
            }

            ZtrFileEntry[] entries = MergeEntries(sourceEntries, targetEntries);

            ZtrFilePacker packer = new ZtrFilePacker(output, InteractionService.TextEncoding.Provide().Encoding);
            packer.Pack(entries);
        }

        private static ZtrFileEntry[] MergeEntries(ZtrFileEntry[] sourceEntries, ZtrFileEntry[] targetEntries)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(targetEntries.Length);
            foreach (ZtrFileEntry entry in targetEntries)
                dic.Add(entry.Key, entry.Value);

            StringBuilder sb = new StringBuilder(1024);
            foreach (ZtrFileEntry entry in sourceEntries)
            {
                string oldText;
                if (!dic.TryGetValue(entry.Key, out oldText))
                {
                    Log.Warning("[ArchiveEntryInjectorStringsToZtr] Пропущена неизвестная запись {0}={1}.", entry.Key, entry.Value);
                    continue;
                }

                if (SkipEntry(oldText))
                    continue;

                string newText = entry.Value;

                GetEndingTags(oldText, sb);
                string oldEnding = sb.ToString();
                sb.Clear();

                int endingLength = GetEndingTags(newText, sb);
                int newLength = newText.Length - endingLength;
                sb.Clear();

                // Восстановление старых хвостов и тегов новой строки
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

        private static bool SkipEntry(string oldText)
        {
            if (string.IsNullOrEmpty(oldText))
                return true;

            //if (oldText.Length < 24)
            //{
            //    foreach (char ch in oldText)
            //        if (ch != ' ' && Char.IsLower(ch))
            //            return false;
            //
            //    return true;
            //}
            //
            return false;
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
                Log.Warning("[ArchiveEntryInjectorStringsToZtr.GetEndingTags] Неверная управляющая последовательность: {0}", text);

            return result;
        }

        private static readonly FFXIIITextTag NewLineTag = new FFXIIITextTag(FFXIIITextTagCode.Text, FFXIIITextTagText.NewLine);
    }
}