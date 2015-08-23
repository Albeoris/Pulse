using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public interface IWdbMovieEntryExtractor
    {
        void Extract(WdbMovieEntry entry, Stream output, Stream content, Byte[] buff);
    }
}