using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pulse.UI
{
    public class UiGrid : Grid
    {
        public void SetRows(int count)
        {
            count -= RowDefinitions.Count;
            if (count > 1) while (count-- > 0) RowDefinitions.Add(new RowDefinition());
        }

        public void SetCols(int count)
        {
            count -= ColumnDefinitions.Count;
            if (count > 1) while (count-- > 0) ColumnDefinitions.Add(new ColumnDefinition());
        }

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

        public T AddUiElement<T>(T uiElement, int row, int col, int rowSpan = 0, int colSpan = 0) where T : UIElement
        {
            if (row > 0) uiElement.SetValue(RowProperty, row);
            if (col > 0) uiElement.SetValue(ColumnProperty, col);
            if (rowSpan > 0) uiElement.SetValue(RowSpanProperty, rowSpan);
            if (colSpan > 0) uiElement.SetValue(ColumnSpanProperty, colSpan);

            Children.Add(uiElement);
            return uiElement;
        }

        public UiGridSplitter AddVerticalSplitter(int col, int row = 0, int rowSpan = 0)
        {
            UiGridSplitter splitter = UiGridSplitterFactory.Create();
            {
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.HorizontalAlignment = HorizontalAlignment.Center;
            }

            AddUiElement(splitter, row, col, rowSpan);

            return splitter;
        }

        public UiGridSplitter AddHorizontalSplitter(int row, int col = 0, int colSpan = 0)
        {
            UiGridSplitter splitter = UiGridSplitterFactory.Create();
            {
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.VerticalAlignment = VerticalAlignment.Center;
            }

            AddUiElement(splitter, row, col, 0, colSpan);

            return splitter;
        }
    }
}