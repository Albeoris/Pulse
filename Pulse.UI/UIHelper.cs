using System;
using System.Text;
using System.Windows;
using Pulse.Core;

namespace Pulse.UI
{
    public static class UiHelper
    {
        public static void ShowError(FrameworkElement owner, Exception exception, string formatMessage = null, params object[] args)
        {
            Log.Error(exception, formatMessage, args);

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(formatMessage))
                sb.AppendFormatLine(formatMessage, args);
            if (exception != null)
                sb.Append(exception);

            if (owner == null)
            {
                MessageBox.Show(sb.ToString(), Lang.Message.Error.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Window window = (Window)owner.GetRootElement();
                MessageBox.Show(window, sb.ToString(), Lang.Message.Error.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string FormatBytes(long value)
        {
            int i = 0;
            decimal dec = value;
            while ((dec > 1024))
            {
                dec /= 1024;
                i++;
            }

            switch (i)
            {
                case 0:
                    return string.Format("{0:F2} " + Lang.Measurement.ByteAbbr, dec);
                case 1:
                    return string.Format("{0:F2} " + Lang.Measurement.KByteAbbr, dec);
                case 2:
                    return string.Format("{0:F2} " + Lang.Measurement.MByteAbbr, dec);
                case 3:
                    return string.Format("{0:F2} " + Lang.Measurement.GByteAbbr, dec);
                case 4:
                    return string.Format("{0:F2} " + Lang.Measurement.TByteAbbr, dec);
                case 5:
                    return string.Format("{0:F2} " + Lang.Measurement.PByteAbbr, dec);
                case 6:
                    return string.Format("{0:F2} " + Lang.Measurement.EByteAbbr, dec);
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}