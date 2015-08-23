namespace Pulse.UI
{
    public static class UiIntegerUpDownFactory
    {
        public static UiIntegerUpDown Create(int? minimum, int? maximum)
        {
            return new UiIntegerUpDown
            {
                Minimum = minimum,
                Maximum = maximum
            };
        }
    }
}