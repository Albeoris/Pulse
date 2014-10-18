using System;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UIHelper
    {
        public static void ShowError(Exception exception, string formatMessage = null, params object[] args)
        {
            Log.Error(exception, formatMessage, args);

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(formatMessage))
                sb.AppendFormatLine(formatMessage, args);
            if (exception != null)
                sb.Append(exception);

            MessageBox.Show(sb.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}