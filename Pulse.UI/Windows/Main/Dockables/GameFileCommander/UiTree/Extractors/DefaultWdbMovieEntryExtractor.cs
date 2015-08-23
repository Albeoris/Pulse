using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DefaultWdbMovieEntryExtractor : IWdbMovieEntryExtractor
    {
        public void Extract(WdbMovieEntry entry, Stream output, Stream content, Byte[] buff)
        {
            content.SetPosition(entry.Offset);
            content.CopyToStream(output, entry.Length, buff);
        }
    }
}