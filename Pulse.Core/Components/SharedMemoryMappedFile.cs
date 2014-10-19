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
        private int _counter;
        private MemoryMappedFile _mmf;

        public SharedMemoryMappedFile(string filePath)
        {
            _filePath = filePath;
        }

        public Stream CreateViewStream()
        {
            IDisposable context = Acquire();
            DisposableStream result = new DisposableStream(_mmf.CreateViewStream(0L, 0L, MemoryMappedFileAccess.ReadWrite));
            result.AfterDispose.Add(context);
            return result;
        }

        public Stream RecreateFile()
        {
            return File.Create(_filePath);
        }

        public Stream CreateViewStream(long offset, long size)
        {
            IDisposable context = Acquire();
            DisposableStream result = new DisposableStream(_mmf.CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite));
            result.AfterDispose.Add(context);
            return result;
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

        private IDisposable Acquire(int size)
        {
            lock (_lock)
            {
                if (Interlocked.Increment(ref _counter) == 1)
                    _mmf = MemoryMappedFile.CreateFromFile(File.Create(_filePath, size), null, size, MemoryMappedFileAccess.ReadWrite, null, HandleInheritability.Inheritable, false);
            }
            return new DisposableAction(Free);
        }

        private void Free()
        {
            lock (_lock)
            {
                if (Interlocked.Decrement(ref _counter) == 0)
                    _mmf.Dispose();
            }
        }
    }
}