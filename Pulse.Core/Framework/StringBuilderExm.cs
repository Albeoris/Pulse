using System.Text;

namespace Pulse.Core
{
    public static class StringBuilderExm
    {
        public static StringBuilder AppendFormatLine(this StringBuilder self, string format, params object[] args)
        {
            Exceptions.CheckArgumentNull(self, "self");

            self.AppendFormat(format, args);
            self.AppendLine();

            return self;
        }
    }
}
