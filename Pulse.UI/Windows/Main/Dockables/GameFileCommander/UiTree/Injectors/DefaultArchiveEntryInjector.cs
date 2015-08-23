using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DefaultArchiveEntryInjector : IArchiveEntryInjector
    {
        public string SourceExtension
        {
            get { return String.Empty; }
        }

        public bool TryInject(IUiInjectionSource source, string sourceFullPath, ArchiveEntryInjectionData data, ArchiveEntry entry)
        {
            using (Stream input = source.TryOpen(sourceFullPath))
            {
                if (input == null)
                    return false;

                using (Stream output = data.OuputStreamFactory(entry))
                input.CopyToStream(output, (int)input.Length, data.Buffer);

                return true;
            }
        }
    }
}