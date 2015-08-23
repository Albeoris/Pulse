using System;
using System.Collections.Generic;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWdbMovieExtractor : IDisposable
    {
        private readonly WdbMovieArchiveListing _listing;
        private readonly WdbMovieEntry[] _leafs;
        private readonly Boolean? _conversion;
        private readonly IUiExtractionTarget _target;
        private readonly Dictionary<String, IWdbMovieEntryExtractor> _extractors;

        public UiWdbMovieExtractor(WdbMovieArchiveListing listing, WdbMovieEntry[] leafs, bool? conversion, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
            _target = target;
            _extractors = ProvideExtractors(conversion);
        }

        public void Dispose()
        {
        }

        public void Extract()
        {
            if (_leafs.Length == 0)
                return;

            String movieDirectory = InteractionService.GameLocation.Provide().MovieDirectory;
            String packagePostfix = _listing.PackagePostfix;

            String root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();
            String targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            _target.CreateDirectory(targetDirectory);

            byte[] buff = new byte[32 * 1024];
            foreach (WdbMovieEntry entry in _leafs)
            {
                String targetExtension;
                IWdbMovieEntryExtractor extractor = GetExtractor(entry, out targetExtension);
                if (extractor == null)
                    return;

                String packageDirectory = Path.Combine(targetDirectory, entry.PackageName);
                Directory.CreateDirectory(packageDirectory);

                String sourcePath = Path.Combine(movieDirectory, entry.PackageName + packagePostfix + ".win32.wmp");
                String targetPath = Path.Combine(packageDirectory, entry.Entry.NameWithoutExtension + '.' + targetExtension);
                using (Stream input = File.OpenRead(sourcePath))
                using (Stream output = _target.Create(targetPath))
                    extractor.Extract(entry, output, input, buff);
            }
        }

        private IWdbMovieEntryExtractor GetExtractor(WdbMovieEntry entry, out String targetExtension)
        {
            targetExtension = "bk2";

            return DefaultExtractor;
        }

        private Dictionary<String, IWdbMovieEntryExtractor> ProvideExtractors(bool? conversion)
        {
            return Empty;
        }

        #region Static

        private static readonly IWdbMovieEntryExtractor DefaultExtractor = ProvideDefaultExtractor();
        private static readonly Dictionary<String, IWdbMovieEntryExtractor> Empty = new Dictionary<String, IWdbMovieEntryExtractor>(0);

        private static IWdbMovieEntryExtractor ProvideDefaultExtractor()
        {
            return new DefaultWdbMovieEntryExtractor();
        }

        #endregion
    }
}