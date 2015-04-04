using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;
using zlib;

namespace Pulse.FS
{
    public static class ZLibHelper
    {
        public static byte[] Uncompress(Stream input, int uncompressedSize)
        {
            byte[] buff = new byte[uncompressedSize];
            ZInputStream reader = new ZInputStream(input);
            reader.EnsureRead(buff, 0, uncompressedSize);
            return buff;
        }

        public static void UncompressAndDisposeStreams(Stream input, Stream output, int uncompressedSize, CancellationToken cancelationToken, Action<long> uncompressed = null)
        {
            try
            {
                if (cancelationToken.IsCancellationRequested)
                    return;

                byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];
                Uncompress(input, output, uncompressedSize, buff, cancelationToken, uncompressed);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                output.SafeDispose();
                input.SafeDispose();
            }
        }

        public static void Uncompress(Stream input, Stream output, int uncompressedSize, byte[] buff, CancellationToken cancelationToken, Action<long> uncompressed = null)
        {
            Exceptions.CheckArgumentNull(input, "input");
            Exceptions.CheckArgumentNull(output, "output");
            Exceptions.CheckArgumentOutOfRangeException(uncompressedSize, "uncompressedSize", 0, int.MaxValue);

            ZInputStream reader = new ZInputStream(input);

            int readed;
            while (uncompressedSize > 0 && (readed = reader.read(buff, 0, Math.Min(buff.Length, uncompressedSize))) > 0)
            {
                if (cancelationToken.IsCancellationRequested)
                    return;

                uncompressedSize -= readed;
                output.Write(buff, 0, readed);
                uncompressed.NullSafeInvoke(readed);
            }

            if (uncompressedSize != 0)
                throw new Exception("Неожиданный конец потока.");
        }

        public static int Compress(Stream input, Stream output, int uncompressedSize, Action<long> compressed = null)
        {
            Exceptions.CheckArgumentNull(input, "input");
            Exceptions.CheckArgumentNull(output, "output");
            Exceptions.CheckArgumentOutOfRangeException(uncompressedSize, "uncompressedSize", 0, int.MaxValue);

            byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];
            ZOutputStream writer = new ZOutputStream(output, 6);

            long position = output.Position;

            int readed;
            while (uncompressedSize > 0 && (readed = input.Read(buff, 0, Math.Min(buff.Length, uncompressedSize))) > 0)
            {
                uncompressedSize -= readed;
                writer.Write(buff, 0, readed);
                compressed.NullSafeInvoke(readed);
            }

            writer.finish();

            if (uncompressedSize != 0)
                throw new Exception("Неожиданный конец потока.");

            return (int)(output.Position - position);
        }

        public static void EnsureRead(this ZInputStream self, byte[] buff, int offset, int size)
        {
            Exceptions.CheckArgumentNull(self, "self");

            int readed;
            while (size > 0 && (readed = self.read(buff, offset, size)) != 0)
            {
                if (readed < 0)
                    throw new Exception("Неожиданный конец потока.");

                size -= readed;
                offset += readed;
            }

            if (size != 0)
                throw new Exception("Неожиданный конец потока.");
        }
    }
}