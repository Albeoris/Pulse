using System;
using System.Windows;
using Pulse.Core;
using Pulse.UI;

namespace Pulse
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class UiGamePartSelectDialog
    {
        public FFXIIIGamePart Result { get; set; }

        public UiGamePartSelectDialog()
        {
            InitializeComponent();
        }

        private void OnPart1ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = FFXIIIGamePart.Part1;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }

        private void OnPart2ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = FFXIIIGamePart.Part2;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }

        private void OnPart3ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = FFXIIIGamePart.Part3;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }
    }
}