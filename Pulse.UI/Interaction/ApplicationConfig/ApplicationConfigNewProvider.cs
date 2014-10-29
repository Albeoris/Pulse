namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigNewProvider : IInfoProvider<ApplicationConfigInfo>
    {
        public ApplicationConfigInfo Provide()
        {
            return new ApplicationConfigInfo();
        }

        public string Title
        {
            get { return "�����"; }
        }

        public string Description {
            get { return "�������������� ��� ��������� ���������� �� ���������."; }
        }
    }
}