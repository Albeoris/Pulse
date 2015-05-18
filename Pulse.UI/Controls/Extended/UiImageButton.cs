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
            
            Content = Image;
        }

        public ImageSource ImageSource
        {
            get { return Image.Source; }
            set { Image.Source = value; }
        }
    }
}