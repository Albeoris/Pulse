using System.Windows;

namespace Pulse.UI
{
    public static class FrameworkElementExm
    {
        public static FrameworkElement GetRootElement(this FrameworkElement self)
        {
            FrameworkElement element = self;
            while (element.Parent != null)
                element = (FrameworkElement)element.Parent;
            return element;
        }

        public static T GetParentElement<T>(this FrameworkElement self) where T : FrameworkElement
        {
            FrameworkElement element = self;
            while (element.Parent != null)
            {
                element = (FrameworkElement)element.Parent;
                T result = element as T;
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}