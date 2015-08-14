using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;
using Pulse.DirectX;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Toolkit.Graphics;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Color = System.Windows.Media.Color;
using Device = SharpDX.Direct3D11.Device;

// ReSharper disable UnusedMember.Local

namespace Pulse.UI
{
    public sealed class UiGameFilePreviewZtr : UiGrid
    {
        private readonly UiTextBox _textBox;

        public UiGameFilePreviewZtr()
        {
            #region Constructor

            _textBox = new UiTextBox {TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, IsReadOnly = true, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto};
            AddUiElement(_textBox, 0, 0);

            #endregion
        }

        private ArchiveListing _listing;
        private ArchiveEntry _entry;

        public void Show(ArchiveListing listing, ArchiveEntry entry)
        {
            _listing = listing;
            _entry = entry;

            if (listing == null || entry == null)
            {
                _textBox.Text = null;
                return;
            }

            FFXIIITextEncoding encoding = InteractionService.TextEncoding.Provide().Encoding;

            ZtrFileEntry[] entries;
            using (Stream data = listing.Accessor.ExtractBinary(entry))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(data, encoding);
                entries = unpacker.Unpack();
            }

            if (entries.IsNullOrEmpty())
                return;

            using (MemoryStream ms = new MemoryStream(4096))
            {
                ZtrTextWriter writer = new ZtrTextWriter(ms, StringsZtrFormatter.Instance);
                writer.Write(entry.Name, entries);

                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms, System.Text.Encoding.UTF8, false))
                    _textBox.Text = sr.ReadToEnd();
            }

            Visibility = Visibility.Visible;
        }
    }
}