using System;
using System.IO;
using System.Text;

namespace Pulse.Core
{
    public class FormattingStreamWriter : StreamWriter
    {
        public override IFormatProvider FormatProvider { get; }

        public FormattingStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen, IFormatProvider formatProvider)
            : base(stream, encoding, bufferSize, leaveOpen)
        {
            FormatProvider = formatProvider;
        }
    }
}