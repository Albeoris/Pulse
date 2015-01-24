using System;
using System.IO;

namespace Pulse.Core
{
    /// <summary>
    /// НЕ потокобезопасный!
    /// </summary>
    public sealed class StreamSegment : Stream, IPositionProvider
    {
        private long _offset, _length;

        public readonly Stream BaseStream;

        public StreamSegment(Stream stream, long offset, long length, FileAccess access)
        {
            Exceptions.CheckArgumentNull(stream, "stream");
            if (offset < 0 || offset >= stream.Length)
                throw new ArgumentOutOfRangeException("offset", offset, "Смещение выходит за границы потока.");
            if (offset + length > stream.Length)
                throw new ArgumentOutOfRangeException("length", length, "Недопустимая длина.");

            _offset = offset;
            _length = length;

            BaseStream = stream;
            switch (access)
            {
                case FileAccess.Read:
                    BaseStream.SetReadPosition(_offset);
                    break;
                case FileAccess.Write:
                    BaseStream.SetWritePosition(_offset);
                    break;
                default:
                    BaseStream.Seek(_offset, SeekOrigin.Begin);
                    break;
            }
        }

        public override bool CanRead
        {
            get { return BaseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return BaseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return BaseStream.CanWrite; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return BaseStream.Position - _offset; }
            set { BaseStream.Position = value + _offset; }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        public void SetOffset(long value)
        {
            _offset = value;
        }

        public override void SetLength(long value)
        {
            _length = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, (int)Math.Min(count, Length - GetReadPosition()));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public long GetReadPosition()
        {
            return BaseStream.GetReadPosition() - _offset;
        }

        public long GetWritePosition()
        {
            return BaseStream.GetWritePosition() - _offset;
        }

        public void SetReadPosition(long position)
        {
            BaseStream.SetReadPosition(position + _offset);
        }

        public void SetWritePosition(long position)
        {
            BaseStream.SetWritePosition(position + _offset);
        }
    }
}