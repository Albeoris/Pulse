using System.IO;
using System.Windows;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFilePreview : UiMainDockableControl
    {
        private readonly UiDxTextureViewer _textureViewer;
        private readonly UiGrid _grid;
        private readonly UiAudioPlayback _sound;
        private readonly UiGameFilePreviewYkd _ykd;
        private readonly UiGameFilePreviewZtr _ztr;

        public UiGameFilePreview()
        {
            #region Construct

            Header = Lang.Dockable.GameFilePreview.Header;

            _grid = UiGridFactory.Create(1, 1);

            _textureViewer = new UiDxTextureViewer();
            _sound = new UiAudioPlayback();
            _ykd = new UiGameFilePreviewYkd();
            _ztr = new UiGameFilePreviewZtr();
            HideControls();

            _grid.AddUiElement(_textureViewer, 0, 0);
            _grid.AddUiElement(_sound, 0, 0);
            _grid.AddUiElement(_ykd, 0, 0);
            _grid.AddUiElement(_ztr, 0, 0);

            Content = _grid;

            #endregion

            InteractionService.SelectedLeafChanged += OnSelectedLeafChanged;
        }

        protected override int Index
        {
            get { return 2; }
        }

        private void HideControls()
        {
            _textureViewer.Visibility = Visibility.Hidden;
            _sound.Visibility = Visibility.Hidden;
            _ykd.Visibility = Visibility.Hidden;
            _ztr.Visibility = Visibility.Hidden;
        }

        private void OnSelectedLeafChanged(IUiLeaf leaf)
        {
            HideControls();

            UiWpdTableLeaf wpdLeaf = leaf as UiWpdTableLeaf;
            if (wpdLeaf != null)
            {
                OnWpdLeafSelected(wpdLeaf);
                return;
            }

            UiArchiveLeaf archiveLeaf = leaf as UiArchiveLeaf;
            if (archiveLeaf != null)
                OnArchiveLeafSelected(archiveLeaf);
        }

        private void OnWpdLeafSelected(UiWpdTableLeaf wpdLeaf)
        {
            WpdEntry entry = wpdLeaf.Entry;
            WpdArchiveListing listing = wpdLeaf.Listing;

            switch (entry.Extension)
            {
                case "txbh":
                case "vtex":
                    _textureViewer.Show(listing, entry);
                    break;
                case "ykd":
                    _ykd.Show(listing, entry);
                    break;
            }
        }

        private void OnArchiveLeafSelected(UiArchiveLeaf archiveLeaf)
        {
            ArchiveEntry entry = archiveLeaf.Entry;
            ArchiveListing listing = archiveLeaf.Listing;

            switch (Path.GetExtension(entry.Name))
            {
                case ".scd":
                    _sound.Show(listing, entry);
                    break;
                case ".ztr":
                    _ztr.Show(listing, entry);
                    break;
            }
        }
    }
}