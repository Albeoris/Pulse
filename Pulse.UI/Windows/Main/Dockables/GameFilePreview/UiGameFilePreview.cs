using System;
using System.IO;
using System.Windows;
using Examples.TextureLoaders;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;
using Pulse.UI.Controls;

namespace Pulse.UI
{
    public sealed class UiGameFilePreview : UiMainDockableControl
    {
        private readonly UiGlTextureViewer _viewer;
        private readonly UiGrid _grid;
        private readonly DisposableStack _disposables = new DisposableStack();
        //private readonly UiGameFilePreviewYkd _ykd;

        public UiGameFilePreview()
        {
            #region Construct

            Header = Lang.Dockable.GameFilePreview.Header;

            _grid = UiGridFactory.Create(1, 1);

            _viewer = new UiGlTextureViewer();
        //    _ykd = new UiGameFilePreviewYkd();
            
            _grid.AddUiElement(_viewer, 0, 0);
            //_grid.AddUiElement(_ykd, 0, 0);

            Content = _grid;

            #endregion

            InteractionService.SelectedLeafChanged += OnSelectedLeafChanged;
        }

        protected override int Index
        {
            get { return 2; }
        }

        private void OnSelectedLeafChanged(IUiLeaf leaf)
        {
            _disposables.Dispose();

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
                    Txbh(listing, entry);
                    break;
                case "ykd":
                    //_ykd.Show(listing, entry);
                    break;

            }
        }

        private void Txbh(WpdArchiveListing listing, WpdEntry entry)
        {
            using (Stream headers = listing.Accessor.ExtractHeaders())
            using (Stream content = listing.Accessor.ExtractContent())
            {
                headers.SetPosition(entry.Offset);

                SectionHeader sectionHeader = headers.ReadContent<SectionHeader>();
                TextureHeader textureHeader = headers.ReadContent<TextureHeader>();
                GtexData gtex = headers.ReadContent<GtexData>();
                if (gtex.Header.LayerCount == 0)
                    return;

                GLTexture texture;
                GtexMipMapLocation mimMap = gtex.MipMapData[0];
                using (StreamSegment textureInput = new StreamSegment(content, mimMap.Offset, mimMap.Length, FileAccess.Read))
                using (GLService.AcquireContext())
                    texture = _disposables.Add(ImageDDS.LoadFromStream(textureInput, gtex));

                ShowTexture(texture);
            }
        }

        private void ShowTexture(GLTexture texture)
        {
            _viewer.Texture = texture;
            Dispatcher.NecessityInvoke(() => _grid.Visibility = Visibility.Visible);
        }
    }
}