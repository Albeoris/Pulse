using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI
{
    public class UiMenu : Menu
    {
        public UiMenuItem AddChild(UiMenuItem item)
        {
            Exceptions.CheckArgumentNull(item, "item");
            Items.Add(item);
            return item;
        }
    }
}