using System;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdLeafsAccessor : IUiLeafsAccessor
    {
        private readonly WpdArchiveListing _listing;
        private readonly WpdEntry[] _leafs;
        private readonly bool? _conversion;

        public UiNodeType Type
        {
            get { return UiNodeType.DataTable; }
        }

        public UiWpdLeafsAccessor(WpdArchiveListing listing, bool? conversion, params WpdEntry[] leafs)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
        }

        public void Extract(IUiExtractionTarget target)
        {
            using (UiWpdExtractor extractor = new UiWpdExtractor(_listing, _leafs, _conversion, target))
                extractor.Extract();
        }

        public void Inject(IUiInjectionSource source, UiInjectionManager manager)
        {
            using (UiWpdInjector injector = new UiWpdInjector(_listing, _leafs, _conversion, source))
                injector.Inject(manager);
        }
    }
}