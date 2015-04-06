using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Pulse.Core
{
    public static class StreamExm
    {
        public static bool IsBeginOfStream(this Stream self)
        {
            return self.Position == 0;
        }

        public static bool IsEndOfStream(this Stream self)
        {
            return self.Position >= self.Length;
        }

        public static void SetPosition(this Stream self, long position)
        {
            if (self.Position != position)
                self.Position = position;
        }

        public static void EnsureRead(this Stream self, byte[] buff, int offset, int size)
        {
            Exceptions.CheckArgumentNull(self, "self");

            int readed;
            while (size > 0 && (readed = self.Read(buff, offset, size)) != 0)
            {
                size -= readed;
                offset += readed;
            }

            if (size != 0)
                throw new Exception("Неожиданный конец потока.");
        }

        public static byte[] EnsureRead(this Stream self, int size)
        {
            byte[] buff = new byte[size];
            EnsureRead(self, buff, 0, size);
            return buff;
        }

        public static void CopyToStream(this Stream input, Stream output, int size, byte[] buff, Action<long> progress = null, bool flush = true)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");
            if (buff == null)
                throw new ArgumentNullException("buff");

            if (!input.CanRead)
            {
                if (!input.CanWrite)
                    throw new ObjectDisposedException("input", "Stream closed.");
                throw new NotSupportedException("Unreadable stream.");
            }

            if (!output.CanWrite)
            {
                if (!output.CanRead)
                    throw new ObjectDisposedException("output", "Stream closed.");
                throw new NotSupportedException("Unwritable stream.");
            }

            int read;
            int left = size;
            while (left > 0 && (read = input.Read(buff, 0, Math.Min(buff.Length, left))) != 0)
            {
                output.Write(buff, 0, read);
                left -= read;
                progress.NullSafeInvoke(read);
            }

            if (left != 0)
                throw new Exception(String.Format("Unexpected end of stream: {0}/{1}", size - left, size));

            if (flush)
                output.Flush();
        }

        public static T[] ReadStructs<T>(this Stream input, int count) where T : new()
        {
            if (input == null)
                throw new ArgumentNullException("input");

            T[] result = new T[count];

            int size = Marshal.SizeOf(TypeCache<T>.Type);
            byte[] buff = new byte[size];
            using (SafeGCHandle handle = new SafeGCHandle(buff, GCHandleType.Pinned))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    input.EnsureRead(buff, 0, size);
                    result[i] = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), TypeCache<T>.Type);
                }
            }

            return result;
        }

        public static T ReadStruct<T>(this Stream input) where T : new()
        {
            if (input == null)
                throw new ArgumentNullException("input");

            return ReadStructs<T>(input, 1)[0];
        }

        public static SafeUnmanagedArray ReadBuff(this Stream input, int size)
        {
            SafeUnmanagedArray handle = new SafeUnmanagedArray(size);

            try
            {
                using (UnmanagedMemoryStream output = new UnmanagedMemoryStream(handle, 0, size, FileAccess.Write))
                {
                    byte[] buff = new byte[Math.Min(32 * 1024, size)];
                    input.CopyToStream(output, size, buff);
                }
            }
            catch
            {
                handle.Dispose();
                throw;
            }

            return handle;
        }

        public static void Read(this Stream input, SafeUnmanagedArray buffer, long offset, int length)
        {
            try
            {
                using (UnmanagedMemoryStream output = new UnmanagedMemoryStream(buffer, 0, length, FileAccess.Write))
                {
                    byte[] buff = new byte[Math.Min(32 * 1024, length)];
                    input.CopyToStream(output, length, buff);
                }
            }
            catch
            {
                buffer.Dispose();
                throw;
            }
        }

        public static T ReadContent<T>(this Stream input) where T : IStreamingContent, new()
        {
            T result = new T();
            result.ReadFromStream(input);
            return result;
        }

        public static T[] ReadContent<T>(this Stream input, int count) where T : IStreamingContent, new()
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadContent<T>(input);
            return result;
        }

        public static void WriteContent<T>(this Stream output, T item) where T : IStreamingContent
        {
            item.WriteToStream(output);
        }

        public static void WriteContent<T>(this Stream output, T[] items) where T : IStreamingContent
        {
            foreach (T item in items)
                item.WriteToStream(output);
        }

        public static void WriteStruct(this Stream output, object pack)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            int size = Marshal.SizeOf(pack);
            byte[] buff = new byte[size];

            using (SafeGCHandle handle = new SafeGCHandle(buff, GCHandleType.Pinned))
            {
                Marshal.StructureToPtr(pack, handle.AddrOfPinnedObject(), false);
                output.Write(buff, 0, size);
            }
        }

        public static BinaryReader GetBinaryReader(this Stream input, Encoding encoding = null)
        {
            return new BinaryReader(Exceptions.CheckArgumentNull(input, "input"), encoding ?? Encoding.Default, true);
        }

        public static BinaryWriter GetBinaryWriter(this Stream output, Encoding encoding = null)
        {
            return new BinaryWriter(Exceptions.CheckArgumentNull(output, "output"), encoding ?? Encoding.Default, true);
        }

        public static StreamReader GetStreamReader(this Stream input, Encoding encoding = null)
        {
            return new StreamReader(Exceptions.CheckArgumentNull(input, "input"), encoding ?? Encoding.Default, true, 4096, true);
        }

        public static StreamSegment GetStreamSegment(this Stream output, long offset, long size = -1)
        {
            return new StreamSegment(output, offset, size < 0 ? output.Length - offset : size, FileAccess.ReadWrite);
        }
    }
}