using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiGridSplitterFactory
    {
        public static UiGridSplitter Create()
        {
            UiGridSplitter splitter = new UiGridSplitter {Width = 5};

            return splitter;
        }
    }
}