using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiButtonFactory
    {
        public static UiButton Create(string title)
        {
            Exceptions.CheckArgumentNullOrEmprty(title, "title");

            return new UiButton {Content = title};
        }
    }
}