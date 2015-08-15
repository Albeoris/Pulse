using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using NAudio.Wave;
using NAudioDemo.AudioPlaybackDemo;
using Pulse.FS;

namespace Pulse.UI
{
    public class UiAudioPlayback : UiGrid
    {
        private readonly UiListView _listView;
        private readonly UiAudioPlayer _audioPlayer;

        public UiAudioPlayback()
        {
            SetCols(2);
            ColumnDefinitions[0].Width = new GridLength(50);

            _listView = UiListViewFactory.Create();
            _listView.DisplayMemberPath = "Title";
            _listView.SelectionChanged += OnListViewSelectionChanged;
            AddUiElement(_listView, 0, 0);

            _audioPlayer = new UiAudioPlayer
            {
                VerticalAlignment = VerticalAlignment.Top,
                Height = 32
            };
            AddUiElement(_audioPlayer, 0, 1);
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListItem item = (e.AddedItems.Count > 0 ? e.AddedItems[0] : null) as ListItem;
            _audioPlayer.SetWave(item?.Provider);
        }

        private ArchiveListing _listing;
        private ArchiveEntry _entry;

        public void Show(ArchiveListing listing, ArchiveEntry entry)
        {
            _listing = listing;
            _entry = entry;

            Visibility = Visibility.Visible;

            using (Stream input = listing.Accessor.ExtractBinary(entry))
            {
                ScdFileReader reader = new ScdFileReader(input);
                WaveStream[] waveProviders = reader.Read();
                ListItem[] items = new ListItem[waveProviders.Length];
                for (int i = 0; i < items.Length; i++)
                    items[i] = new ListItem(i.ToString("D3"), waveProviders[i]);
                _listView.ItemsSource = items;
                if (items.Length > 0)
                    _listView.SelectedIndex = 0;
            }
        }

        private sealed class ListItem
        {
            public string Title { get; private set; }
            public WaveStream Provider { get; private set; }

            public ListItem(string title, WaveStream provider)
            {
                Title = title;
                Provider = provider;
            }
        }
    }
}