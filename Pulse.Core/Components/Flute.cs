using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Pulse.Core
{
    public sealed class Flute : IDisposable
    {
        public const int BlockSize = 64 * 1024 * 1024;

        private readonly MemoryMappedFile _mmf;
        private readonly int[] _occupancy;

        private long _writers, _readers;

        public Flute(long capacity)
        {
            string name = Guid.NewGuid().ToString();
            _mmf = MemoryMappedFile.CreateNew(name, capacity);
            _occupancy = new int[(capacity / BlockSize) + 1];
        }

        public void Dispose()
        {
            _mmf.SafeDispose();
        }

        public static void CreatePipe(long capacity, out Stream input, out Stream output)
        {
            using (DisposableStack insurance = new DisposableStack(3))
            {
                Flute flute = insurance.Add(new Flute(capacity));
                Stream writer = insurance.Add(flute.AcquireWriter(0, capacity));
                Stream reader = insurance.Add(flute.AcquireReader(0, capacity));

                DisposableStream disposableOutput = new DisposableStream(reader);
                disposableOutput.AfterDispose.Add(flute);
                disposableOutput.AfterDispose.Add(writer);

                input = writer;
                output = disposableOutput;

                insurance.Clear();
            }
        }

        public Stream AcquireWriter()
        {
            return new FluteWriter(this);
        }

        public Stream AcquireWriter(long offset, long size)
        {
            long blockOffset = offset % BlockSize;
            if (blockOffset == 0)
                return new FluteWriter(this, offset, size);

            if (blockOffset < GetReadableSize(offset))
                return new FluteWriter(this, offset, size);

            String error = $"Запись по смещению {offset} невозможна, так как блок данных содержит пустоты.";
            throw new NotSupportedException(error);
        }

        public Stream AcquireReader()
        {
            return new FluteReader(this);
        }

        public Stream AcquireReader(long offset, long size)
        {
            return new FluteReader(this, offset, size);
        }

        private void SetReadableSize(long position, long count)
        {
            int beginIndex = (int)position / BlockSize;

            position += count;
            int endIndex = (int)position / BlockSize;
            int endOffset = (int)(position % BlockSize);

            while (beginIndex < endIndex)
                _occupancy[beginIndex++] = BlockSize;

            _occupancy[endIndex] = endOffset;
        }

        private int GetReadableSize(long position)
        {
            long blockIndex = position / BlockSize;
            long blockOffset = position % BlockSize;

            if (Interlocked.Read(ref _writers) == 0)
            {
                int readableBlockSize = _occupancy[blockIndex];
                int result = (int)(readableBlockSize - blockOffset);
                if (result < 0)
                    throw new Exception();

                return result == 0 ? -1 : result;
            }
            else
            {
                int readableBlockSize = _occupancy[blockIndex];
                return (int)(readableBlockSize - blockOffset);
            }
        }

        private sealed class FluteWriter : Stream
        {
            private readonly Flute _flute;
            private readonly MemoryMappedViewStream _output;
            private readonly Queue<long> _writingQueue = new Queue<long>();
            private long _disposed;

            public FluteWriter(Flute flute, long offset = 0, long size = 0)
            {
                _flute = flute;
                _output = _flute._mmf.CreateViewStream(offset, size, MemoryMappedFileAccess.Write);
                Interlocked.Increment(ref _flute._writers);
            }

            protected override void Dispose(bool disposing)
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return;

                if (disposing)
                {
                    Invoker.SafeInvoke(Flush);
                    _output.SafeDispose();
                }

                Interlocked.Decrement(ref _flute._writers);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException("Поток не поддерживает чтение.");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (Interlocked.Read(ref _disposed) == 1)
                    throw new ObjectDisposedException("Поток был освобождён.");

                long position = _output.PointerOffset + Position;
                _output.Write(buffer, offset, count);
                _writingQueue.Enqueue(position);
                _writingQueue.Enqueue(count);
            }

            public override void Flush()
            {
                lock (_writingQueue)
                {
                    _output.Flush();

                    while (_writingQueue.Count > 0)
                    {
                        long position = _writingQueue.Dequeue();
                        long count = _writingQueue.Dequeue();

                        _flute.SetReadableSize(position, count);
                    }
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException("Поток не поддерживает поиск.");
            }

            public override void SetLength(long value)
            {
                _output.SetLength(value);
            }

            public override long Position
            {
                get { return _output.Position; }
                set { throw new NotSupportedException("Поток не поддерживает поиск."); }
            }

            public override long Length => _output.Length;
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => _output.CanWrite;
        }

        private sealed class FluteReader : Stream
        {
            private readonly Flute _flute;
            private readonly MemoryMappedViewStream _input;
            private long _disposed;

            public FluteReader(Flute flute, long offset = 0, long size = 0)
            {
                _flute = flute;
                _input = _flute._mmf.CreateViewStream(offset, size, MemoryMappedFileAccess.Read);
                Interlocked.Increment(ref _flute._readers);
            }

            protected override void Dispose(bool disposing)
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return;

                if (disposing)
                    _input.SafeDispose();

                Interlocked.Decrement(ref _flute._readers);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (Interlocked.Read(ref _disposed) == 1)
                    throw new ObjectDisposedException("Поток был освобождён.");

                if (count == 0)
                    return 0;

                int timeout = 10;
                int readableSize = 0;
                for (int i = 0; i < 100; i++)
                {
                    readableSize = Math.Min(count, _flute.GetReadableSize(_input.PointerOffset + Position));
                    if (readableSize == -1)
                        return 0;
                    if (readableSize > 0)
                        break;

                    Thread.Sleep(timeout); // Максимум 10 * 50(50+1)/2 = 50 сек 500 мсек
                    timeout += 10;
                }

                return _input.Read(buffer, offset, readableSize);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException("Поток не поддерживает запись. ");
            }

            public override void Flush()
            {
                _input.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _input.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _input.SetLength(value);
            }

            public override long Position
            {
                get { return _input.Position; }
                set { _input.Position = value; }
            }

            public override long Length => _input.Length;
            public override bool CanRead => _input.CanRead;
            public override bool CanSeek => _input.CanSeek;
            public override bool CanWrite => false;
        }
    }
}