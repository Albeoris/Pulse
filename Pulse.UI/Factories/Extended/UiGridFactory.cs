using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiGridFactory
    {
        public static UiGrid Create(int rows, int cols)
        {
            Exceptions.CheckArgumentOutOfRangeException(rows, "rows", 1, 1024);
            Exceptions.CheckArgumentOutOfRangeException(cols, "cols", 1, 1024);
            UiGrid grid = new UiGrid();

            if (rows > 1) while (rows-- > 0) grid.RowDefinitions.Add(new RowDefinition());
            if (cols > 1) while (cols-- > 0) grid.ColumnDefinitions.Add(new ColumnDefinition());

            return grid;
        }
    }
}