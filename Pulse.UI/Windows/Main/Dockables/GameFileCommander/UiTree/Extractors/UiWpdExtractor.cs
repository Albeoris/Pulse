using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdExtractor : IDisposable
    {
        private readonly WpdArchiveListing _listing;
        private readonly WpdEntry[] _leafs;
        private readonly Boolean? _conversion;
        private readonly IUiExtractionTarget _target;
        private readonly Dictionary<string, IWpdEntryExtractor> _extractors;
        private readonly Lazy<Stream> _headers;
        private readonly Lazy<Stream> _content;

        public UiWpdExtractor(WpdArchiveListing listing, WpdEntry[] leafs, bool? conversion, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
            _target = target;
            _extractors = ProvideExtractors(conversion);
            _headers = new Lazy<Stream>(AcquireHeaders);
            _content = new Lazy<Stream>(AcquireContent);
        }

        public void Dispose()
        {
            _headers.NullSafeDispose();
            _content.NullSafeDispose();
        }

        public void Extract()
        {
            if (_leafs.Length == 0)
                return;

            String root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();
            String targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            _target.CreateDirectory(targetDirectory);

            byte[] buff = new byte[32 * 1024];
            foreach (WpdEntry entry in _leafs)
            {
                String targetExtension;
                IWpdEntryExtractor extractor = GetExtractor(entry, out targetExtension);
                if (extractor == null)
                    return;

                String targetPath = Path.Combine(targetDirectory, entry.NameWithoutExtension + '.' + targetExtension);
                using (Stream output = _target.Create(targetPath))
                    extractor.Extract(entry, output, _headers, _content, buff);
            }
        }

        private IWpdEntryExtractor GetExtractor(WpdEntry entry, out String targetExtension)
        {
            targetExtension = entry.Extension.ToLowerInvariant();

            IWpdEntryExtractor result;
            if (_extractors.TryGetValue(targetExtension, out result))
                targetExtension = result.TargetExtension;
            else if (_conversion != true)
                result = DefaultExtractor;

            return result;
        }

        private Dictionary<String, IWpdEntryExtractor> ProvideExtractors(bool? conversion)
        {
            return conversion == false ? Empty : Converters;
        }

        private Stream AcquireHeaders()
        {
            return _listing.Accessor.ExtractHeaders();
        }
        
        private Stream AcquireContent()
        {
            return _listing.Accessor.ExtractContent();
        }

        #region Static

        private static readonly IWpdEntryExtractor DefaultExtractor = ProvideDefaultExtractor();
        private static readonly Dictionary<String, IWpdEntryExtractor> Empty = new Dictionary<String, IWpdEntryExtractor>(0);
        private static readonly Dictionary<String, IWpdEntryExtractor> Converters = RegisterConverters();

        private static IWpdEntryExtractor ProvideDefaultExtractor()
        {
            return new DefaultWpdEntryExtractor();
        }

        private static Dictionary<String, IWpdEntryExtractor> RegisterConverters()
        {
            return new Dictionary<String, IWpdEntryExtractor>
            {
                {"txbh", new TxbhToDdsWpdEntryExtractor()},
                {"vtex", new VtexToDdsWpdEntryExtractor()}
            };
        }

        #endregion
    }
}