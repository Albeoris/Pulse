using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderSettingsWindow : UiWindow
    {
        public UiGameFileCommanderSettingsWindow(bool isExtracting)
        {
            #region Construct

            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;

            UiGrid root = UiGridFactory.Create(3, 1);
            {
                Thickness margin = new Thickness(3);

                UiStackPanel maskPanel = UiStackPanelFactory.Create(Orientation.Horizontal);
                {
                    UiTextBlock maskLabel = UiTextBlockFactory.Create("Маска: ");
                    {
                        maskLabel.Margin = margin;
                        maskLabel.VerticalAlignment = VerticalAlignment.Center;
                        maskPanel.AddUiElement(maskLabel);
                    }

                    _wildcardBox = UiTextBoxFactory.Create("*");
                    {
                        _wildcardBox.Width = 300;
                        _wildcardBox.Margin = margin;
                        maskPanel.AddUiElement(_wildcardBox);
                    }

                    root.AddUiElement(maskPanel, 0, 0);
                }

                UiStackPanel settingsPanel = UiStackPanelFactory.Create(Orientation.Horizontal);
                {
                    if (!isExtracting)
                    {
                        _compressBox = UiCheckBoxFactory.Create("Сжать", false);
                        {
                            _compressBox.Margin = margin;
                            _compressBox.IsThreeState = true;
                            _compressBox.IsChecked = null;
                            settingsPanel.AddUiElement(_compressBox);
                        }
                    }

                    _convertBox = UiCheckBoxFactory.Create("Конвертировать", false);
                    {
                        _convertBox.Margin = margin;
                        _convertBox.IsThreeState = true;
                        _convertBox.IsChecked = null;
                        settingsPanel.AddUiElement(_convertBox);
                    }

                    root.AddUiElement(settingsPanel, 1, 0);
                }

                UiStackPanel buttonsPanel = UiStackPanelFactory.Create(Orientation.Horizontal);
                {
                    buttonsPanel.HorizontalAlignment = HorizontalAlignment.Right;

                    UiButton okButton = UiButtonFactory.Create("OK");
                    {
                        okButton.Width = 100;
                        okButton.Margin = margin;
                        okButton.Click += OnOkButtonClick;
                        buttonsPanel.AddUiElement(okButton);
                    }

                    UiButton cancelButton = UiButtonFactory.Create("Отмена");
                    {
                        cancelButton.Width = 100;
                        cancelButton.Margin = margin;
                        cancelButton.Click += OnCancelButtonClick;
                        buttonsPanel.AddUiElement(cancelButton);
                    }

                    root.AddUiElement(buttonsPanel, 2, 0);
                }
            }

            Content = root;

            #endregion
        }

        private readonly UiTextBox _wildcardBox;
        private readonly UiCheckBox _compressBox;
        private readonly UiCheckBox _convertBox;

        public string Wildcard { get; private set; }
        public bool? Compression { get; private set; }
        public bool? Convert { get; private set; }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Wildcard = _wildcardBox.Text;
            if (_compressBox != null) Compression = _compressBox.IsChecked;
            if (_convertBox != null) Convert = _convertBox.IsChecked;
            
            DialogResult = true;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}