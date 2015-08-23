using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiNodePath
    {
        public readonly UiNodePathElement[] Elements;

        public UiNodePath(UiNodePathElement[] elements)
        {
            Elements = elements;
        }

        public UiNodePathElement this[int level] => level < Elements.Length ? Elements[level] : null;

        public bool IsLast(int level)
        {
            return Elements.Length - 1 == level;
        }
    }

    public sealed class UiNodePathElement
    {
        public UiNodeType[] Types;
        public Wildcard[] Wildcards;
        public Boolean IsMandatory;

        public bool IsMatch(UiNode node)
        {
            if (!IsMandatory)
                return true;
            if (Types != null && !Types.Contains(node.Type))
                return false;
            if (Wildcards != null && !Wildcards.Any(w => w.IsMatch(node.Name)))
                return false;

            return true;
        }
    }

    public sealed class UiNodePathBuilder
    {
        private readonly List<UiNodePathElement> _elements;

        public UiNodePathBuilder(int capacity = 0)
        {
            _elements = capacity == 0 ? new List<UiNodePathElement>() : new List<UiNodePathElement>(capacity);
        }

        public UiNodePath Build()
        {
            return new UiNodePath(_elements.ToArray());
        }

        public void Add(UiNodePathElement element)
        {
            _elements.Add(element);
        }

        public void Add()
        {
            Add(new UiNodePathElement {IsMandatory = false});
        }

        public void Add(UiNodeType type)
        {
            Add(new UiNodePathElement {Types = new[] {type}, IsMandatory = true});
        }

        public void Add(Wildcard wildcard)
        {
            Add(new UiNodePathElement {Wildcards = new[] {wildcard}, IsMandatory = true});
        }

        public void Add(UiNodeType type, Wildcard wildcard)
        {
            Add(new UiNodePathElement {Types = new[] {type}, Wildcards = new[] {wildcard}, IsMandatory = true});
        }
    }

    public sealed class UiArchives : IEnumerable<UiContainerNode>
    {
        private readonly UiArchiveNode[] _nodes;

        public UiArchives(UiArchiveNode[] nodes)
        {
            _nodes = nodes;
        }

        public int Count => _nodes.Length;

        public IEnumerator<UiContainerNode> GetEnumerator()
        {
            if (_nodes == null)
                yield break;

            foreach (UiArchiveNode node in _nodes)
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
            return EnumerateCheckedLeafs(wildcard).GroupBy(a => a.Type).SelectMany(group => AcessToLeafs(group, conversion, compression));
        }

        public IEnumerable<IUiLeafsAccessor> AcessToLeafs(IGrouping<UiNodeType, IUiLeaf> group, bool? conversion, bool? compression)
        {
            switch (group.Key)
            {
                case UiNodeType.ArchiveLeaf:
                {
                    foreach (UiArciveLeafsAccessor accessor in GroupArchiveLeafs(group.OfType<UiArchiveLeaf>(), conversion, compression))
                        yield return accessor;
                    break;
                }
                case UiNodeType.FileTableLeaf:
                {
                    foreach (UiWpdLeafsAccessor accessor in GroupWpdLeafs(group.OfType<UiWpdTableLeaf>(), conversion)) // TODO: SEDBLeafs
                        yield return accessor;
                    break;
                }
                case UiNodeType.DataTableLeaf:
                {
                    foreach (UiWdbMovieLeafsAccessor accessor in GroupWdbLeafs(group.OfType<UiWdbMovieLeaf>(), conversion))
                        yield return accessor;
                    break;
                }
                default:
                {
                    throw new NotImplementedException(group.Key.ToString());
                }
            }
        }

        private IEnumerable<UiArciveLeafsAccessor> GroupArchiveLeafs(IEnumerable<UiArchiveLeaf> leafs, bool? conversion, bool? compression)
        {
            foreach (IGrouping<ArchiveListing, UiArchiveLeaf> group in leafs.GroupBy(l => l.Listing))
                yield return new UiArciveLeafsAccessor(group.Key, conversion, compression, group.Select(l => l.Entry).ToArray());
        }

        private IEnumerable<UiWpdLeafsAccessor> GroupWpdLeafs(IEnumerable<UiWpdTableLeaf> leafs, bool? conversion)
        {
            foreach (IGrouping<WpdArchiveListing, UiWpdTableLeaf> group in leafs.GroupBy(l => l.Listing))
                yield return new UiWpdLeafsAccessor(group.Key, conversion, group.Select(l => l.Entry).ToArray());
        }

        private IEnumerable<UiWdbMovieLeafsAccessor> GroupWdbLeafs(IEnumerable<UiWdbMovieLeaf> leafs, bool? conversion)
        {
            foreach (IGrouping<WdbMovieArchiveListing, UiWdbMovieLeaf> group in leafs.GroupBy(l => l.Listing))
                yield return new UiWdbMovieLeafsAccessor(group.Key, conversion, group.Select(l => l.Entry).ToArray());
        }
    }
}