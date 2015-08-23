using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Pulse.UI.Annotations;

namespace Pulse.UI
{
    public sealed class UiDataProviderNode : INotifyPropertyChanged
    {
        private DrawingImage _icon = Icons.PendingIcon;

        public string Title { get; private set; }
        public string Description { get; private set; }
        public UiContextMenu ContextMenu { get; private set; }

        public DrawingImage Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private UiDataProviderNode(string title, string description, UiContextMenu contextMenu)
        {
            Title = title;
            Description = description;
            ContextMenu = contextMenu;
        }

        public static UiDataProviderNode Create<T>(InfoProviderGroup<T> providers) where T : class
        {
            UiContextMenu menu = UiContextMenuFactory.Create();
            for (int index = 0; index < providers.Count; index++)
            {
                IInfoProvider<T> provider = providers[index];
                UiMenuItem menuItem = UiMenuItemFactory.Create(provider.Title, new UiDataProviderNodeRefreshCommand(() => providers.Refresh(provider)));
                menuItem.ToolTip = provider.Description;
                menu.AddChild(menuItem);
            }

            UiDataProviderNode node = new UiDataProviderNode(providers.Title, providers.Description, menu);

            providers.InfoLost += node.OnInfoLost;
            providers.InfoProvided += node.OnInfoProvided;

            return node;
        }

        private void OnInfoLost()
        {
            Icon = Icons.CrossIcon;
        }

        private void OnInfoProvided(object value)
        {
            Icon = Icons.OkIcon;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}