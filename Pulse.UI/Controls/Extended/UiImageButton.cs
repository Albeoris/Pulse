using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pulse.UI
{
    public class UiImageButton : UiButton
    {
        public readonly Image Image;

        public UiImageButton()
        {
            Image = new Image();
            Background = Brushes.Transparent;
            BorderBrush = Brushes.Transparent;
            Template = StaticTemplate;

            Content = Image;
        }

        public ImageSource ImageSource
        {
            get { return Image.Source; }
            set { Image.Source = value; }
        }

        private static readonly ControlTemplate StaticTemplate = CreateTemplate();

        private static ControlTemplate CreateTemplate()
        {
            ControlTemplate result = new ControlTemplate(typeof(UiImageButton));

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));

            result.VisualTree = contentPresenter;
            return result;
        }
    }
}