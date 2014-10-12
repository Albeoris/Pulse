using System.Windows.Input;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiMenuItemFactory
    {
        public static UiMenuItem Create(string title, ICommand command = null, object commandParameter = null)
        {
            Exceptions.CheckArgumentNullOrEmprty(title, "title");

            UiMenuItem menuItem = new UiMenuItem {Header = title, Command = command, CommandParameter = commandParameter};

            return menuItem;
        }
    }
}