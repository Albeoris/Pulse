using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Pulse.Core
{
    public sealed class SharedMemoryMappedFile
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private long _counter;
        private MemoryMappedFile _mmf;

        public SharedMemoryMappedFile(string filePath)
        {
            _filePath = filePath;
        }

        public Stream RecreateFile()
        {
            lock (_lock)
            {
                if (Interlocked.Read(ref _counter) != 0)
                    throw new NotSupportedException();
                return File.Create(_filePath);
            }
        }

        public Stream CreateViewStream(long offset, long size, MemoryMappedFileAccess access)
        {
            IDisposable context = Acquire();
            DisposableStream result = new DisposableStream(_mmf.CreateViewStream(offset, size, access));
            result.AfterDispose.Add(context);
            return result;
        }

        private IDisposable Acquire()
        {
            lock (_lock)
            {
                if (Interlocked.Increment(ref _counter) == 1)
                    _mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
            }
            return new DisposableAction(Free);
        }

        private void Free()
        {
            lock (_lock)
            {
                if (Interlocked.Decrement(ref _counter) == 0)
                    _mmf.Dispose();
                Monitor.PulseAll(_lock);
            }
        }

        public Stream IncreaseSize(int value, out long offset)
        {
            lock (_lock)
            {
                while (Interlocked.Read(ref _counter) != 0)
                {
                    if (!Monitor.Wait(_lock, 10000, true))
                        throw new NotSupportedException();
                }

                FileStream file = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                try
                {
                    offset = MathEx.RoundUp(file.Length, 0x800);
                    file.SetLength(offset + value);
                    _mmf = MemoryMappedFile.CreateFromFile(file, null, 0, MemoryMappedFileAccess.ReadWrite, null, HandleInheritability.Inheritable, false);
                }
                catch
                {
                    file.SafeDispose();
                    throw;
                }

                Interlocked.Increment(ref _counter);
                DisposableStream result = new DisposableStream(_mmf.CreateViewStream(offset, value, MemoryMappedFileAccess.ReadWrite));
                result.AfterDispose.Add(new DisposableAction(Free));
                return result;
            }
        }
    }
}