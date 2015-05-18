using System.Windows.Controls;
using System.Windows.Media;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiButtonFactory
    {
        public static UiButton Create(string content)
        {
            Exceptions.CheckArgumentNullOrEmprty(content, "content");

            return new UiButton {Content = content};
        }
    }
}