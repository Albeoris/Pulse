using System;
using System.IO;
using System.Threading;

namespace Pulse.Core
{
    public sealed class ProxyMemoryStream : Stream, IPositionProvider
    {
        private const int Timeout = 5000;
        
        private readonly DisposableStack _disposables = new DisposableStack(3);
        private readonly AutoResetEvent _readyForRead = new AutoResetEvent(false);

        private readonly SafeUnmanagedArray _buff;
        private readonly UnmanagedMemoryStream _write, _read;

        public ProxyMemoryStream(int size)
        {
            try
            {
                _buff = _disposables.Add(new SafeUnmanagedArray(size));
                _write = _disposables.Add(_buff.OpenStream(FileAccess.Write));
                _read = _disposables.Add(_buff.OpenStream(FileAccess.Read));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _disposables.Dispose();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_write.Position != _write.Length)
            {
                if (!_readyForRead.WaitOne(Timeout))
                    throw new TimeoutException();
            }

            long witePosition = _write.Position;
            _readyForRead.Reset();

            int capacity = (int)(witePosition - _read.Position);
            int size = Math.Min(count, capacity);

            int readed = _read.Read(buffer, offset, size);
            if ((readed > 0 && readed < capacity) || witePosition != _write.Position)
                _readyForRead.Set();

            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            _write.Write(buffer, offset, count);
            _write.Flush();
            _readyForRead.Set();
        }

        public override void Flush()
        {
            _write.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return _buff.Length; }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public long GetReadPosition()
        {
            return _read.Position;
        }

        public long GetWritePosition()
        {
            return _write.Position;
        }

        public void SetReadPosition(long position)
        {
            if (position > Length)
                throw new ArgumentOutOfRangeException("position");

            int index = 0;
            while (true)
            {
                if (!_readyForRead.WaitOne(Timeout) && index++ > 10)
                    throw new TimeoutException();

                if (_write.Position > position)
                {
                    _read.Position = position;
                    _readyForRead.Set();
                    break;
                }
            }
        }

        public void SetWritePosition(long position)
        {
            throw new NotSupportedException();
        }
    }
}