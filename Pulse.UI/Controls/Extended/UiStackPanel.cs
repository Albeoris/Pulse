using System.Windows;
using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI
{
    public class UiStackPanel : StackPanel
    {
        public T AddUiElement<T>(T uiElement) where T : UIElement
        {
            Exceptions.CheckArgumentNull(uiElement, "uiElement");

            Children.Add(uiElement);
            return uiElement;
        }
    }
}