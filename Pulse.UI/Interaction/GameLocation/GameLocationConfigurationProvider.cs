namespace Pulse.UI
{
    public sealed class GameLocationConfigurationProvider : IInfoProvider<GameLocationInfo>
    {
        public GameLocationInfo Provide()
        {
            GameLocationInfo value = InteractionService.Configuration.Provide().GameLocation;
            value.Validate();
            return value;
        }

        public string Title
        {
            get { return "�� ��������"; }
        }

        public string Description
        {
            get { return "���������� ����������, ���������� � ����������� �����."; }
        }
    }
}