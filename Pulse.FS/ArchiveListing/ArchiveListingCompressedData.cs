using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingCompressedData : IStreamingContent
    {
        private readonly IArchiveListingHeader _header;

        private readonly byte[][] _uncompressedBlocks;
        private volatile Exception _exception;

        public ArchiveListingCompressedData(IArchiveListingHeader header)
        {
            _header = header;
            _uncompressedBlocks = new byte[_header.BlocksCount][];
        }

        public void ReadFromStream(Stream stream)
        {
            stream.Position = _header.BlockOffset;
            ArchiveListingBlockInfo[] blocks = stream.ReadStructs<ArchiveListingBlockInfo>(_header.BlocksCount);

            ThreadHelper.StartBackground("ArchiveListingCompressedData.ReadFromStream", () => UncompressBlocks(blocks, stream));
        }

        public byte[] AcquireData(int blockNumber)
        {
            while (_exception == null)
            {
                byte[] result = _uncompressedBlocks[blockNumber];
                if (result != null)
                    return result;
                
                Thread.Sleep(100);
            }

            throw _exception;
        }

        private void UncompressBlocks(ArchiveListingBlockInfo[] blocks, Stream stream)
        {
            try
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    ArchiveListingBlockInfo block = blocks[i];
                    int uncompressedSize = block.UncompressedSize;

                    stream.Position = _header.InfoOffset + block.Offset;
                    _uncompressedBlocks[i] = ZLibHelper.Uncompress(stream, uncompressedSize);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        public void WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}