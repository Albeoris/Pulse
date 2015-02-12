using System.Collections;
using System.Collections.Generic;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchives : IEnumerable<UiArchiveNode>
    {
        private readonly UiArchiveNode[] _nodes;

        public UiArchives(UiArchiveNode[] nodes)
        {
            _nodes = nodes;
        }

        public IEnumerator<UiArchiveNode> GetEnumerator()
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

        public IEnumerable<IArchiveListing> EnumerateCheckedEntries(Wildcard wildcard)
        {
            foreach (UiArchiveNode archive in this)
            {
                if (archive.IsChecked == false)
                    continue;

                foreach (IArchiveListing child in archive.CreateChildListing((ArchiveListing)archive.Listing, wildcard))
                {
                    if (child.Count > 0)
                        yield return child;
                }
            }
        }
    }
}