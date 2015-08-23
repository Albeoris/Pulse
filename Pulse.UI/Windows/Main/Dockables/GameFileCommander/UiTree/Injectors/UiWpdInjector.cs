using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdInjector : IDisposable
    {
        private readonly WpdArchiveListing _listing;
        private readonly WpdEntry[] _leafs;
        private readonly IUiInjectionSource _source;
        private readonly Boolean? _conversion;
        private readonly Dictionary<string, IWpdEntryInjector> _injectors;
        private readonly Lazy<Stream> _headers;
        private readonly Lazy<Stream> _content;
        private readonly Byte[] _buff = new Byte[32 * 1024];
        private Boolean _injected;

        public UiWpdInjector(WpdArchiveListing listing, WpdEntry[] leafs, bool? conversion, IUiInjectionSource source)
        {
            _listing = listing;
            _leafs = leafs;
            _source = source;
            _conversion = conversion;
            _injectors = ProvideInjectors();
            _headers = new Lazy<Stream>(AcquireHeaders);
            _content = new Lazy<Stream>(AcquireContent);
        }

        public void Dispose()
        {
            _headers.NullSafeDispose();
            _content.NullSafeDispose();
        }

        public void Inject(UiInjectionManager manager)
        {
            if (_leafs.Length == 0)
                return;

            String root = _source.ProvideRootDirectory();
            String targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            if (!_source.DirectoryIsExists(targetDirectory))
                return;

            foreach (WpdEntry entry  in _leafs)
            {
                String targetPath = Path.Combine(targetDirectory, entry.NameWithoutExtension);
                Inject(entry, targetPath);
            }

            if (_injected)
            {
                List<ArchiveEntry> entries = new List<ArchiveEntry>(2);
                MemoryInjectionSource memorySource = new MemoryInjectionSource();
                if (_headers.IsValueCreated)
                {
                    ArchiveEntry entry = _listing.Accessor.HeadersEntry;
                    entries.Add(entry);
                    memorySource.RegisterStream(entry.Name, _headers.Value);
                }
                if (_content.IsValueCreated)
                {
                    ArchiveEntry entry = _listing.Accessor.ContentEntry;
                    entries.Add(entry);
                    memorySource.RegisterStream(entry.Name, _content.Value);
                }

                using (UiArchiveInjector injector = new UiArchiveInjector(_listing.Accessor.Parent, entries.ToArray(), _conversion, false, memorySource))
                    injector.Inject(manager);

                manager.Enqueue(_listing.Accessor.Parent);
            }
        }

        private void Inject(WpdEntry entry, String targetPath)
        {
            string targetExtension = entry.Extension.ToLowerInvariant();

            IWpdEntryInjector injector;
            if (_injectors.TryGetValue(targetExtension, out injector))
            {
                string targetFullPath = targetPath + '.' + injector.SourceExtension;
                using (Stream input = _source.TryOpen(targetFullPath))
                {
                    if (input != null)
                    {
                        injector.Inject(entry, input, _headers, _content, _buff);
                        _injected = true;
                        return;
                    }
                }
            }

            if (_conversion != true)
            {
                string targetFullPath = targetPath + '.' + targetExtension;
                using (Stream input = _source.TryOpen(targetFullPath))
                {
                    if (input != null)
                    {
                        DefaultInjector.Inject(entry, input, _headers, _content, _buff);
                        _injected = true;
                    }
                }
            }
        }

        private Dictionary<String, IWpdEntryInjector> ProvideInjectors()
        {
            return _conversion != false ? Converters : Emptry;
        }

        private Stream AcquireHeaders()
        {
            int uncompressedSize = (int)_listing.Accessor.HeadersEntry.UncompressedSize;

            MemoryStream result = new MemoryStream((int)(uncompressedSize * 1.3));
            using (Stream input = _listing.Accessor.ExtractHeaders())
                input.CopyToStream(result, uncompressedSize, _buff);

            return result;
        }

        private Stream AcquireContent()
        {
            int uncompressedSize = (int)_listing.Accessor.ContentEntry.UncompressedSize;

            MemoryStream result = new MemoryStream((int)(uncompressedSize * 1.3));
            using (Stream input = _listing.Accessor.ExtractContent())
                input.CopyToStream(result, uncompressedSize, _buff);

            return result;
        }

        #region Static

        private static readonly IWpdEntryInjector DefaultInjector = ProvideDefaultInjector();
        private static readonly Dictionary<String, IWpdEntryInjector> Emptry = new Dictionary<String, IWpdEntryInjector>(0);
        private static readonly Dictionary<String, IWpdEntryInjector> Converters = RegisterConverters();

        private static IWpdEntryInjector ProvideDefaultInjector()
        {
            return new DefaultWpdEntryInjector();
        }

        private static Dictionary<String, IWpdEntryInjector> RegisterConverters()
        {
            return new Dictionary<String, IWpdEntryInjector>
            {
                {"txbh", new DdsToTxbhWpdEntryInjector()},
                {"vtex", new DdsToVtexWpdEntryInjector()}
            };
        }

        #endregion

        public static void InjectSingle(WpdArchiveListing listing, WpdEntry entry, MemoryStream output)
        {
            using (MemoryInjectionSource source = new MemoryInjectionSource())
            {
                source.RegisterStream(String.Empty, output);
                UiWpdInjector injector = new UiWpdInjector(listing, new[] {entry}, false, source);

                UiInjectionManager manager = new UiInjectionManager();
                injector.Inject(manager);
                manager.WriteListings();
            }
        }
    }
}