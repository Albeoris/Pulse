using System.Windows.Controls;

namespace Pulse.UI
{
    public class UiListView : ListView
    {
        public void FocusSelectedItem()
        {
            int index = SelectedIndex;
            if (index < 0)
                return;

            UpdateLayout();
            ((ListBoxItem)ItemContainerGenerator.ContainerFromIndex(index)).Focus();
        }
    }
}