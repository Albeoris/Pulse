using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArciveLeafsAccessor : IUiLeafsAccessor
    {
        private readonly ArchiveListing _listing;
        private readonly ArchiveEntry[] _leafs;
        private readonly bool? _conversion;
        private readonly bool? _compression;

        public UiNodeType Type
        {
            get { return UiNodeType.Archive; }
        }

        public UiArciveLeafsAccessor(ArchiveListing listing, bool? conversion, bool? compression, params ArchiveEntry[] leafs)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
            _compression = compression;
        }

        public void Extract(IUiExtractionTarget target)
        {
            using (UiArchiveExtractor extractor = new UiArchiveExtractor(_listing, _leafs, _conversion, target))
                extractor.Extract();
        }

        public void Inject(IUiInjectionSource source, UiInjectionManager manager)
        {
            using (UiArchiveInjector injector = new UiArchiveInjector(_listing, _leafs, _conversion, _compression, source))
                injector.Inject(manager);
        }
    }
}