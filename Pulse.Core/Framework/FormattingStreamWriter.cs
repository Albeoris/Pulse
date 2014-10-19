using System;
using System.IO;
using System.Text;

namespace Pulse.Core
{
    public class FormattingStreamWriter : StreamWriter
    {
        private readonly IFormatProvider _formatProvider;

        public FormattingStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen, IFormatProvider formatProvider)
            : base(stream, encoding, bufferSize, leaveOpen)
        {
            _formatProvider = formatProvider;
        }

        public override IFormatProvider FormatProvider
        {
            get { return _formatProvider; }
        }
    }
}