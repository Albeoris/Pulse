using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchives : IEnumerable<UiContainerNode>
    {
        private readonly UiContainerNode[] _nodes;

        public UiArchives(UiContainerNode[] nodes)
        {
            _nodes = nodes;
        }

        public int Count
        {
            get { return _nodes.Length; }
        }

        public IEnumerator<UiContainerNode> GetEnumerator()
        {
            if (_nodes == null)
                yield break;

            foreach (UiContainerNode node in _nodes)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<IUiLeaf> EnumerateCheckedLeafs(Wildcard wildcard)
        {
            foreach (UiContainerNode archive in this)
            {
                if (archive.IsChecked == false)
                    continue;

                foreach (IUiLeaf child in archive.EnumerateCheckedLeafs(wildcard, archive.IsChecked == true))
                    yield return child;
            }
        }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public IEnumerable<IUiLeafsAccessor> AccessToCheckedLeafs(Wildcard wildcard, bool? conversion, bool? compression)
        {
            foreach (IGrouping<UiNodeType, IUiLeaf> group in EnumerateCheckedLeafs(wildcard).GroupBy(a => a.Type))
            {
                switch (group.Key)
                {
                    case UiNodeType.ArchiveLeaf:
                    {
                        foreach (UiArciveLeafsAccessor accessor in GroupArchiveLeafs(group.OfType<UiArchiveLeaf>(), conversion, compression))
                            yield return accessor;
                        break;
                    }
                    case UiNodeType.DataTableLeaf:
                    {
                        foreach (UiWpdLeafsAccessor accessor in GroupWpdLeafs(group.OfType<UiWpdTableLeaf>(), conversion))
                            yield return accessor;
                        break;
                    }
                    default:
                    {
                        throw new NotImplementedException(group.Key.ToString());
                    }
                }
            }
        }

        private IEnumerable<UiArciveLeafsAccessor> GroupArchiveLeafs(IEnumerable<UiArchiveLeaf> leafs, bool? conversion, bool? compression)
        {
            foreach (IGrouping<ArchiveListing, UiArchiveLeaf> group in leafs.GroupBy(l => l.Listing))
                yield return new UiArciveLeafsAccessor(group.Key, conversion, compression, group.Select(l=>l.Entry).ToArray());
        }

        private IEnumerable<UiWpdLeafsAccessor> GroupWpdLeafs(IEnumerable<UiWpdTableLeaf> leafs, bool? conversion)
        {
            foreach (IGrouping<WpdArchiveListing, UiWpdTableLeaf> group in leafs.GroupBy(l => l.Listing))
                yield return new UiWpdLeafsAccessor(group.Key, conversion, group.Select(l=>l.Entry).ToArray());
        }
    }
}