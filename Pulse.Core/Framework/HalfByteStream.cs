using System;
using System.IO;

namespace Pulse.Core
{
    public sealed class HalfByteStream : Stream
    {
        private long _position;
        private byte? _leftHalf;

        public readonly Stream BaseStream;

        public HalfByteStream(Stream stream)
        {
            Exceptions.CheckArgumentNull(stream, "stream");

            BaseStream = stream;
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
            get { return BaseStream.Length * 2; }
        }

        public override long Position
        {
            get { return _position; }
            set { _position += value; }
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

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value / 2);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int left = count;
            if (_leftHalf != null)
            {
                buffer[offset++] = _leftHalf.Value;
                left--;
                _leftHalf = null;
            }

            if (left == 0)
            {
                _position += count;
                return count;
            }

            byte[] buff = new byte[left / 2 + left % 2];
            int readed = BaseStream.Read(buff, 0, buff.Length);
            for (int i = 0; i < readed; i++)
            {
                buffer[offset + i * 2] = (byte)(buff[i] & 0xF);
                left--;

                if (left != 0)
                {
                    buffer[offset + i * 2 + 1] = (byte)((buff[i] >> 4) & 0xF);
                    left--;
                }
                else
                {
                    _leftHalf = (byte)((buff[i] >> 4) & 0xF);
                    if (i + 1 != readed)
                        throw new Exception("Abnormal error.");
                }
            }

            int shift = count - left;
            _position += shift;
            return shift;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count % 2 != 0) throw new ArgumentException("count");

            for (int i = 0; i < count / 2; i++)
                BaseStream.WriteByte((byte)(buffer[offset + i * 2] | (buffer[offset + i * 2 + 1] << 4)));
        }
    }
}