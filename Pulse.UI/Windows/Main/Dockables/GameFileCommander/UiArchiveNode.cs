using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Pulse.Core.Components;
using Pulse.FS;
using Pulse.UI.Annotations;

namespace Pulse.UI
{
    public sealed class UiArchiveNode : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ArchiveListing Listing { get; set; }
        public ArchiveListingEntry Entry { get; set; }
        public UiArchiveNode Parent { get; set; }
        public UiArchiveNode[] Childs { get; set; }

        public IEnumerable<UiArchiveNode> FolderChilds
        {
            get { return Childs.Where(c => c.Entry == null); }
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

        public ArchiveListing CreateChildListing(Wildcard wildcard)
        {
            ArchiveListing parentListing = GetParentListing();
            ArchiveListing listing = new ArchiveListing(parentListing.Accessor);
            foreach (ArchiveListingEntry entry in EnumerateFiles())
            {
                if (wildcard == null || wildcard.IsMatch(entry.Name))
                    listing.Add(entry);
            }
            listing.TrimExcess();
            return listing;
        }

        private IEnumerable<ArchiveListingEntry> EnumerateFiles()
        {
            if (Entry != null)
                yield return Entry;

            foreach (ArchiveListingEntry entry in Childs.SelectMany(child => child.EnumerateFiles()))
                yield return entry;
        }

        private ArchiveListing GetParentListing()
        {
            UiArchiveNode node = this;
            while (node.Listing == null)
                node = node.Parent;
            return node.Listing;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
    }
}