using Pulse.Core;

namespace Pulse.UI
{
    public static class UiWatermarkTextBoxFactory
    {
        public static UiWatermarkTextBox Create(string watermark, string text = null)
        {
            Exceptions.CheckArgumentNull(watermark, "watermark");

            UiWatermarkTextBox textBlock = new UiWatermarkTextBox {Watermark = watermark, Text = text ?? string.Empty};

            return textBlock;
        }
    }
}