using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI
{
    public class UiGrid : Grid
    {
        public void SetRowsHeight(GridLength height)
        {
            foreach (RowDefinition row in RowDefinitions)
                row.Height = height;
        }

        public void SetColsWidth(GridLength width)
        {
            foreach (ColumnDefinition col in ColumnDefinitions)
                col.Width = width;
        }

        public void AddUiElement(UIElement uiElement, int row, int col, int rowSpan = 0, int colSpan = 0)
        {
            if (row > 0) uiElement.SetValue(RowProperty, row);
            if (col > 0) uiElement.SetValue(ColumnProperty, col);
            if (rowSpan > 0) uiElement.SetValue(RowSpanProperty, rowSpan);
            if (colSpan > 0) uiElement.SetValue(ColumnSpanProperty, colSpan);
            
            Children.Add(uiElement);
        }
    }
}
