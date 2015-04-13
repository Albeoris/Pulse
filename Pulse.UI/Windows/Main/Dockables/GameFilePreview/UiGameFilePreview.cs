using System.Windows;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI.Controls;

namespace Pulse.UI
{
    public sealed class UiGameFilePreview : UiMainDockableControl
    {
        private readonly UiGlTextureViewer _textureViewer;
        private readonly UiGrid _grid;
        private readonly UiGameFilePreviewYkd _ykd;

        public UiGameFilePreview()
        {
            #region Construct

            Header = Lang.Dockable.GameFilePreview.Header;

            _grid = UiGridFactory.Create(1, 1);

            _textureViewer = new UiGlTextureViewer();
            _ykd = new UiGameFilePreviewYkd();
            HideControls();

            _grid.AddUiElement(_textureViewer, 0, 0);
            _grid.AddUiElement(_ykd, 0, 0);

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
            _ykd.Visibility = Visibility.Hidden;
        }

        private void OnSelectedLeafChanged(IUiLeaf leaf)
        {
            HideControls();

            UiWpdTableLeaf wpdLeaf = leaf as UiWpdTableLeaf;
            if (wpdLeaf != null)
                OnWpdLeafSelected(wpdLeaf);
        }

        private void OnWpdLeafSelected(UiWpdTableLeaf wpdLeaf)
        {
            WpdEntry entry = wpdLeaf.Entry;
            WpdArchiveListing listing = wpdLeaf.Listing;

            switch (entry.Extension)
            {
                case "txbh":
                    _textureViewer.Show(listing, entry);
                    break;
                case "ykd":
                    _ykd.Show(listing, entry);
                    break;
            }
        }
    }
}