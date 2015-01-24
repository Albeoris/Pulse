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