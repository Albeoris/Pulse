using System.IO;
using System.IO.MemoryMappedFiles;

namespace Pulse.Core
{
    public sealed class FileSegment : Stream
    {
        private readonly MemoryMappedFile _mmf;
        private readonly Stream _stream;
        private long _length;

        public FileSegment(MemoryMappedFile mmf, long offset, long length, MemoryMappedFileAccess access)
        {
            _mmf = Exceptions.CheckArgumentNull(mmf, "mmf");
            try
            {
                _length = length;
                if (_length == 0)
                    _stream = new MemoryStream();
                else
                    _stream = mmf.CreateViewStream(offset, length, access);
            }
            catch
            {
                _mmf.Dispose();
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _mmf.Dispose();
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }
    }
}