using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public sealed class ProgressArgs
    {
        public readonly object Key;
        public readonly long ProcessedSize, TotalSize;

        public ProgressArgs(object key, long processedSize, long totalSize)
        {
            Key = key;
            ProcessedSize = processedSize;
            TotalSize = totalSize;
        }
    }
}
