using System.Windows.Controls;

namespace Pulse.UI
{
    public static class UiStackPanelFactory
    {
        public static UiStackPanel Create(Orientation orientation)
        {
            UiStackPanel stackPanel = new UiStackPanel
            {
                Orientation = orientation
            };

            return stackPanel;
        }
    }
}