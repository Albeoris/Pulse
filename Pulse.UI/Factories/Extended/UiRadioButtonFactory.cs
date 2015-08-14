namespace Pulse.UI
{
    public static class UiRadioButtonFactory
    {
        public static UiRadioButton Create(string groupName, object content, bool? isChecked)
        {
            return new UiRadioButton
            {
                Content = content,
                IsChecked = isChecked,
                GroupName = groupName
            };
        }
    }
}