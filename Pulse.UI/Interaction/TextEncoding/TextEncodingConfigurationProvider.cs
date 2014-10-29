using System.IO;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingConfigurationProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.Load();
        }

        public string Title
        {
            get { return "�� �������� ��������"; }
        }

        public string Description
        {
            get { return "���������� ����������, ���������� � ����������� �����."; }
        }
    }
}