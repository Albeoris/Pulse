using System;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWdbMovieLeafsAccessor : IUiLeafsAccessor
    {
        private readonly WdbMovieArchiveListing _listing;
        private readonly WdbMovieEntry[] _leafs;
        private readonly bool? _conversion;

        public UiNodeType Type => UiNodeType.FileTable;

        public UiWdbMovieLeafsAccessor(WdbMovieArchiveListing listing, bool? conversion, params WdbMovieEntry[] leafs)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
        }

        public void Extract(IUiExtractionTarget target)
        {
            using (UiWdbMovieExtractor extractor = new UiWdbMovieExtractor(_listing, _leafs, _conversion, target))
                extractor.Extract();
        }

        public void Inject(IUiInjectionSource source, UiInjectionManager manager)
        {
            throw new NotImplementedException();
            //using (UiWpdInjector injector = new UiWpdInjector(_listing, _leafs, _conversion, source))
            //    injector.Inject(manager);
        }
    }
}