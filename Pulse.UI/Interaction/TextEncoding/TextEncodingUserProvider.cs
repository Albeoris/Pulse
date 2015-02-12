using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examples.TextureLoaders;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;
using Pulse.UI.Encoding;

namespace Pulse.UI
{
    public class TextEncodingUserProvider : IInfoProvider<TextEncodingInfo>
    {
        private TextEncodingInfo _oldEncoding;

        public string Title
        {
            get { return Lang.InfoProvider.TextEncoding.UserTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.TextEncoding.UserDescription; }
        }

        public TextEncodingInfo Provide()
        {
            ArchiveListing listing;
            XgrArchiveAccessor accessor = CreateAccessor(out listing);

            XgrArchiveListing fontEntries;
            WflContent[] fontContent;
            TextureSection[] textureHeaders;
            string[] names;
            ReadXgrContent(accessor, listing, out fontEntries, out fontContent, out textureHeaders, out names);

            GLTexture[] textures = ReadTextures(accessor, textureHeaders);
            try
            {
                char[] chars = new char[256 + WflContent.AdditionalTableCount];
                ConcurrentDictionary<char, short> codes = new ConcurrentDictionary<char, short>();
                if (_oldEncoding == null)
                    _oldEncoding = InteractionService.TextEncoding.Provide();

                UiEncodingWindowSource[] sources = PrepareWindowSources(fontContent, textures, names, chars, codes);
                UiEncodingWindow wnd = new UiEncodingWindow();
                foreach (UiEncodingWindowSource source in sources)
                    wnd.Add(source);

                if (wnd.ShowDialog() == true)
                {
                    FFXIIICodePage codepage = new FFXIIICodePage(chars, new Dictionary<char, short>(codes));
                    FFXIIITextEncoding encoding = new FFXIIITextEncoding(codepage);
                    TextEncodingInfo result = new TextEncodingInfo(encoding);

                    result.Save();

                    int index = 0;
                    XgrArchiveInjector injector = new XgrArchiveInjector(fontEntries, false, e => new XgrArchiveEntryInjectorWflContentPack(fontContent[index++], e));
                    injector.Inject();

                    return result;
                }
            }
            finally
            {
                textures.SafeDispose();
            }

            throw new OperationCanceledException();
        }

        //private void Replace(UiEncodingWindowSource[] sources)
        //{
        //    Dictionary<char, char> dic = new Dictionary<char, char>();
        //    dic['А'] = 'A';
        //    dic['A'] = 'А';
        //    dic['В'] = 'B';
        //    dic['B'] = 'В';
        //    dic['Е'] = 'E';
        //    dic['E'] = 'Е';
        //    dic['К'] = 'K';
        //    dic['K'] = 'К';
        //    dic['М'] = 'M';
        //    dic['M'] = 'М';
        //    dic['Н'] = 'H';
        //    dic['H'] = 'Н';
        //    dic['О'] = 'O';
        //    dic['O'] = 'О';
        //    dic['Р'] = 'P';
        //    dic['P'] = 'Р';
        //    dic['С'] = 'C';
        //    dic['C'] = 'С';
        //    dic['Т'] = 'T';
        //    dic['T'] = 'Т';
        //    dic['Х'] = 'X';
        //    dic['X'] = 'Х';
        //    dic['а'] = 'a';
        //    dic['a'] = 'а';
        //    dic['е'] = 'e';
        //    dic['e'] = 'е';
        //    dic['и'] = 'u';
        //    dic['u'] = 'и';
        //    dic['о'] = 'o';
        //    dic['o'] = 'о';
        //    dic['р'] = 'p';
        //    dic['p'] = 'р';
        //    dic['с'] = 'c';
        //    dic['c'] = 'с';
        //    dic['у'] = 'y';
        //    dic['y'] = 'у';
        //    dic['х'] = 'x';
        //    dic['x'] = 'х';
            
        //    int[] sbig = {0x44, 0x46, 0x47, 0x49, 0x4A, 0x4C, 0x4E, 0x51, 0x52, 0x53, 0x55, 0x56, 0x57, 0x59, 0x5A};
        //    int[] slit = {0x62, 0x64, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x71, 0x72, 0x73, 0x74, 0x76, 0x77, 0x7A};
        //    int[] tbig = {0xc1, 0xc3, 0xc4, 0xc7, 0xc8, 0xcb, 0xcf, 0xd3, 0xd4, 0xdf, 0xe1, 0xe2, 0xe3, 0xe4, 0xe6};
        //    int[] tlit = {0xe7, 0xe9, 0xea, 0xeb, 0xec, 0xed, 0xef, 0xf2, 0xf4, 0xf6, 0xf7, 0xf8, 0xf9, 0xfb, 0xfc, 0xfd, 0xfe, 0xff};

        //    UiEncodingWindowSource good = sources[1];
        //    foreach (UiEncodingWindowSource source in sources)
        //    {
        //        if (source == good)
        //            continue;

        //        int index = 0;
        //        for (int rus = 0x44; rus <= 0x5A; rus++)
        //        {
        //            char engChar;
        //            char rusChar = source.Chars[rus];
        //            if (dic.TryGetValue(rusChar, out engChar))
        //            {
        //                short eng = source.Codes[engChar];
        //                source.Codes[rusChar] = eng;
        //                source.Chars[rus] = '\0';
        //            }
        //            else if (rus == sbig[index])
        //            {
        //                int eng = tbig[index++];
        //                Swap(source, eng, rus);
        //            }
        //        }

        //        index = 0;
        //        for (int rus = 0x62; rus <= 0x7A; rus++)
        //        {
        //            char engChar;
        //            char rusChar = source.Chars[rus];
        //            if (dic.TryGetValue(rusChar, out engChar))
        //            {
        //                short eng = source.Codes[engChar];
        //                source.Codes[rusChar] = eng;
        //                source.Chars[rus] = '\0';
        //            }
        //            else if (rus == slit[index])
        //            {
        //                int eng = tlit[index++];
        //                Swap(source, eng, rus);
        //            }
        //        }
        //    }
        //}

        //private void Swap(UiEncodingWindowSource source, int eng, int rus)
        //{
        //    char engCh = source.Chars[eng];
        //    char rusCh = source.Chars[rus];

        //    short engCode, rusCode;
        //    bool e = source.Codes.TryGetValue(engCh, out engCode);
        //    bool r = source.Codes.TryGetValue(rusCh, out rusCode);
        //    if (e)
        //        if (r) source.Codes[engCh] = rusCode;
        //        else source.Codes.Remove(engCh);
        //    if (r)
        //        if (e) source.Codes[rusCh] = engCode;
        //        else source.Codes.Remove(rusCh);

        //    source.Chars.Swap(eng, rus);
        //    source.Info.Offsets.Swap(eng, rus);
        //    source.Info.Sizes.Swap(eng, rus);
        //    source.Info.Offsets.Swap(eng + 256, rus + 256);
        //    source.Info.Sizes.Swap(eng + 256, rus + 256);
        //}

        private UiEncodingWindowSource[] PrepareWindowSources(WflContent[] wflContents, GLTexture[] textures, string[] names, char[] chars, ConcurrentDictionary<char, short> codes)
        {
            if (_oldEncoding != null)
            {
                FFXIIICodePage codepage = _oldEncoding.Encoding.Codepage;
                codepage.Chars.CopyTo(chars, 0);
                foreach (KeyValuePair<char, short> pair in codepage.Codes)
                    codes.TryAdd(pair.Key, pair.Value);
            }

            UiEncodingWindowSource[] result = new UiEncodingWindowSource[names.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = new UiEncodingWindowSource(names[i], textures[i], wflContents[i], chars, codes);

            return result;
        }

        private XgrArchiveAccessor CreateAccessor(out ArchiveListing listing)
        {
            GameLocationInfo gameLocation = InteractionService.GameLocation.Provide();
            string binaryPath = Path.Combine(gameLocation.SystemDirectory, "white_imgc.win32.bin");
            string listingPath = Path.Combine(gameLocation.SystemDirectory, "filelistc.win32.bin");

            ArchiveAccessor accessor = new ArchiveAccessor(binaryPath, listingPath);
            listing = ArchiveListingReader.Read(null, accessor).First();
            ArchiveEntry xgrEntry = listing.Single(n => n.Name.EndsWith(@"gui/resident/system.win32.xgr"));
            ArchiveEntry imgbEntry = listing.Single(n => n.Name.EndsWith(@"gui/resident/system.win32.imgb"));

            return new XgrArchiveAccessor(accessor, xgrEntry, imgbEntry);
        }

        private void ReadXgrContent(XgrArchiveAccessor accessor, ArchiveListing listing, out XgrArchiveListing fontEntries, out WflContent[] fontContent, out TextureSection[] textureHeaders, out string[] names)
        {
            using (Stream indices = accessor.ExtractIndices())
            {
                WpdEntry[] textureEntries;
                ReadFontIndices(accessor, listing, indices, out textureEntries, out fontEntries, out names);

                fontContent = ReadFontContent(indices, fontEntries);
                textureHeaders = ReadTextureHeaders(indices, textureEntries);
            }
        }

        private void ReadFontIndices(XgrArchiveAccessor accessor, ArchiveListing archiveListing, Stream xgr, out WpdEntry[] textureEntries, out XgrArchiveListing fontEntries, out string[] names)
        {
            WpdHeader header = xgr.ReadContent<WpdHeader>();
            XgrArchiveListing listing = new XgrArchiveListing(accessor, header.Count) {ParentArchiveListing = archiveListing};
            listing.AddRange(header.Entries);

            fontEntries = new XgrArchiveListing(listing.Accessor) {FullListing = listing, ParentArchiveListing = listing.ParentArchiveListing};

            List<WpdEntry> textures = new List<WpdEntry>(4);
            List<WpdEntry> fonts = new List<WpdEntry>(4);

            foreach (WpdEntry entry in header.Entries)
            {
                const string prefix = "wfnt";

                if (!entry.Name.StartsWith(prefix))
                    continue;

                switch (entry.Extension)
                {
                    case "txbh":
                        textures.Add(entry);
                        break;
                    case "wfl":
                        fonts.Add(entry);
                        break;
                }
            }

            names = SortAndExcludeNotPaired(textures, fonts);
            fontEntries.AddRange(fonts);
            textureEntries = textures.ToArray();
        }

        private static string[] SortAndExcludeNotPaired(List<WpdEntry> textures, List<WpdEntry> fonts)
        {
            var dic = new Dictionary<String, Pair<WpdEntry, WpdEntry>>(Math.Max(textures.Count, fonts.Count));

            foreach (WpdEntry entry in textures)
            {
                Pair<WpdEntry, WpdEntry> pair;
                string name = entry.Name.Substring(0, 6);
                if (!dic.TryGetValue(name, out pair))
                {
                    pair = new Pair<WpdEntry, WpdEntry>();
                    dic.Add(name, pair);
                }
                pair.Item1 = entry;
            }

            foreach (WpdEntry entry in fonts)
            {
                Pair<WpdEntry, WpdEntry> pair;
                string name = entry.Name.Substring(0, 6);
                if (!dic.TryGetValue(name, out pair))
                {
                    pair = new Pair<WpdEntry, WpdEntry>();
                    dic.Add(name, pair);
                }
                pair.Item2 = entry;
            }

            textures.Clear();
            fonts.Clear();

            List<string> names = new List<string>(dic.Count);
            foreach (var pair in dic.Where(p => !p.Value.IsAnyEmpty).OrderBy(p => p.Key))
            {
                names.Add(pair.Key);
                textures.Add(pair.Value.Item1);
                fonts.Add(pair.Value.Item2);
            }

            return names.ToArray();
        }

        private static WflContent[] ReadFontContent(Stream xgr, XgrArchiveListing fontEntries)
        {
            WflContent[] result = new WflContent[fontEntries.Count];
            for (int i = 0; i < result.Length; i++)
            {
                WpdEntry entry = fontEntries[i];
                using (StreamSegment wflInput = new StreamSegment(xgr, entry.Offset, entry.Length, FileAccess.Read))
                {
                    WflFileReader reader = new WflFileReader(wflInput);
                    result[i] = reader.Read();
                }
            }
            return result;
        }

        private static TextureSection[] ReadTextureHeaders(Stream xgrStream, WpdEntry[] textureEntries)
        {
            TextureSection[] result = new TextureSection[textureEntries.Length];
            for (int i = 0; i < result.Length; i++)
            {
                WpdEntry entry = textureEntries[i];
                using (StreamSegment textureHeaderInput = new StreamSegment(xgrStream, entry.Offset, entry.Length, FileAccess.Read))
                    result[i] = textureHeaderInput.ReadContent<TextureSection>();
            }
            return result;
        }

        private static GLTexture[] ReadTextures(XgrArchiveAccessor accessor, TextureSection[] gtexDatas)
        {
            GLTexture[] textures = new GLTexture[gtexDatas.Length];

            using (Stream imgbStream = accessor.ExtractContent())
            using (DisposableStack insurance = new DisposableStack())
            {
                for (int i = 0; i < gtexDatas.Length; i++)
                {
                    GtexData data = gtexDatas[i].Gtex;
                    textures[i] = insurance.Add(ReadTexture(imgbStream, data));

                }
                insurance.Clear();
            }

            return textures;
        }

        private static GLTexture ReadTexture(Stream imgbStream, GtexData data)
        {
            if (data.Header.LayerCount == 0)
                return null;

            GtexMipMapLocation mimMap = data.MipMapData[0];
            using (StreamSegment textureInput = new StreamSegment(imgbStream, mimMap.Offset, mimMap.Length, FileAccess.Read))
            using (GLService.AcquireContext())
                return ImageDDS.LoadFromStream(textureInput, data);
        }

        public void EncodingProvided(TextEncodingInfo encoding)
        {
            _oldEncoding = encoding;
        }
    }
}