using System.Windows;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

namespace Pulse.UI
{
    public sealed class UiInputWindow : UiWindow
    {
        public UiInputWindow(string watermark, string text)
        {
            #region Construct

            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;

            UiGrid root = UiGridFactory.Create(1, 2);
            {
                root.ColumnDefinitions[1].Width = GridLength.Auto;

                _textBox = UiWatermarkTextBoxFactory.Create(watermark, text);
                {
                    _textBox.Width = 320;
                    _textBox.Margin = new Thickness(3);
                    root.AddUiElement(_textBox, 0, 0);
                }

                UiButton button = UiButtonFactory.Create("OK");
                {
                    button.Width = 70;
                    button.Margin = new Thickness(3);
                    button.Click += OnOkButtonClick;
                    root.AddUiElement(button, 0, 1);
                }
            }
            Content = root;

            #endregion
        }

        private readonly UiWatermarkTextBox _textBox;

        public string Answer { get; private set; }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Answer = _textBox.Text;
            DialogResult = true;
        }
    }
}