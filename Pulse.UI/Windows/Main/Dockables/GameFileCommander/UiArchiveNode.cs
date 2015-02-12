using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI.Annotations;

namespace Pulse.UI
{
    public sealed class UiArchiveNode : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public IArchiveListing Listing { get; set; }
        public IArchiveEntry Entry { get; set; }
        public UiArchiveNode Parent { get; set; }
        public UiArchiveNode[] Childs { get; set; }

        public IEnumerable<UiArchiveNode> OrderedChilds
        {
            get { return Childs.OrderBy(n => n, UiArchiveNodeComparer.Instance); }
        }

        public IEnumerable<UiArchiveNode> OrderedHierarchyChilds // Binding
        {
            get { return Childs.Where(c => c.Entry == null).OrderBy(n => n, UiArchiveNodeComparer.Instance); }
        }

        // Binding
        public BitmapSource Icon
        {
            get
            {
                if (Listing != null) return Icons.DiskIcon;
                if (Entry == null) return Icons.FolderIcon;

                string ext = Path.GetExtension(Name);
                ext = ext == null ? string.Empty : ext.ToLower();
                switch (ext)
                {
                    case ".txt":
                    case ".ztr":
                        return Icons.TxtFileIcon;
                }

                return null;
            }
        }

        public IEnumerable<IArchiveListing> CreateChildListing(ArchiveListing archiveListing, Wildcard wildcard)
        {
            ArchiveListing listing = new ArchiveListing(archiveListing.Accessor) {FullListing = archiveListing};
            foreach (UiArchiveNode child in EnumerateChilds())
            {
                ArchiveEntry entry = child.Entry as ArchiveEntry;
                if (entry != null)
                {
                    if (wildcard == null || wildcard.IsMatch(entry.Name))
                        listing.Add(entry);
                }

                XgrArchiveListing xgr = child.Listing as XgrArchiveListing;
                if (xgr != null)
                    yield return CreateXgrChildListing(child, wildcard, archiveListing);
            }
            listing.TrimExcess();
            yield return listing;
        }

        public XgrArchiveListing CreateXgrChildListing(UiArchiveNode archiveNode, Wildcard wildcard, ArchiveListing archiveListing)
        {
            XgrArchiveListing fullListing = (XgrArchiveListing)archiveNode.Listing;
            XgrArchiveListing listing = new XgrArchiveListing(fullListing.Accessor) {FullListing = fullListing, ParentArchiveListing = archiveListing};
          
            foreach (UiArchiveNode childNode in archiveNode.Childs)
            {
                if (childNode.IsChecked == true && (wildcard == null || wildcard.IsMatch(childNode.Entry.Name)))
                    listing.Add((WpdEntry)childNode.Entry);
            }
            listing.TrimExcess();
            return listing;
        }

        private IEnumerable<UiArchiveNode> EnumerateChilds()
        {
            if (IsChecked == false)
                yield break;

            if (Entry != null || Listing is XgrArchiveListing)
            {
                yield return this;
                yield break;
            }

            foreach (UiArchiveNode entry in Childs.Where(c => c.IsChecked != false).SelectMany(child => child.EnumerateChilds()))
                yield return entry;
        }

        private bool _isSelected;
        private bool _isExpanded;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        #region IsChecked

        private enum CheckedChanger
        {
            Manual,
            Parent,
            Child
        }

        private bool? _isChecked = false;
        private CheckedChanger _checkedChanger = CheckedChanger.Manual;
        private int _checkedCount;
        private int _unknownCount;

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                try
                {
                    if (_isChecked == value)
                        return;

                    SetIsChecked(value);
                    OnPropertyChanged();
                }
                finally
                {
                    _checkedChanger = CheckedChanger.Manual;
                }
            }
        }

        private void SetIsChecked(bool? value)
        {
            if (value == null && _checkedChanger == CheckedChanger.Manual)
                value = !_isChecked;

            if (_checkedChanger != CheckedChanger.Child)
            {
                for (int i = 0; i < Childs.Length; i++)
                {
                    UiArchiveNode entry = Childs[i];
                    entry._checkedChanger = CheckedChanger.Parent;
                    entry.IsChecked = value;
                }
            }

            if (Parent != null)
                Parent.OnChildCheckedChanged(_isChecked, value);

            _isChecked = value;
        }

        private void OnChildCheckedChanged(bool? oldValue, bool? newValue)
        {
            switch (oldValue)
            {
                case null:
                    Interlocked.Decrement(ref _unknownCount);
                    break;
                case true:
                    Interlocked.Decrement(ref _checkedCount);
                    break;
            }

            switch (newValue)
            {
                case null:
                    Interlocked.Increment(ref _unknownCount);
                    break;
                case true:
                    Interlocked.Increment(ref _checkedCount);
                    break;
            }

            _checkedChanger = CheckedChanger.Child;

            try
            {
                if (_unknownCount > 0)
                    IsChecked = null;
                else if (_checkedCount > 0)
                    IsChecked = _checkedCount == Childs.Length ? (bool?)true : null;
                else
                    IsChecked = false;
            }
            finally
            {
                _checkedChanger = CheckedChanger.Manual;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}