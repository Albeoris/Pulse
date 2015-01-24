using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.FS
{
    public sealed class XgrArchiveAccessor
    {
        public readonly ArchiveAccessor Parent;
        public readonly ArchiveEntry XgrIndicesEntry;
        public readonly ArchiveEntry XgrContentEntry;

        public XgrArchiveAccessor(ArchiveAccessor parent, ArchiveEntry xgrIndicesEntry, ArchiveEntry xgrContentEntry)
        {
            Parent = parent;
            XgrIndicesEntry = xgrIndicesEntry;
            XgrContentEntry = xgrContentEntry;
        }

        public string Name
        {
            get { return XgrIndicesEntry.Name; }
        }

        public Stream ExtractIndices()
        {
            return Parent.ExtractBinary(XgrIndicesEntry);
        }

        public Stream ExtractContent()
        {
            return Parent.ExtractBinary(XgrContentEntry);
        }

        public Stream RecreateIndices(int newSize)
        {
            return Parent.OpenOrAppendBinary(XgrIndicesEntry, newSize);
        }

        public Stream RecreateContent(int newSize)
        {
            return Parent.OpenOrAppendBinary(XgrContentEntry, newSize);
        }
    }
}