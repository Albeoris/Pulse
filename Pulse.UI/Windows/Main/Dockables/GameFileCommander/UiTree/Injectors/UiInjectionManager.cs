using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiInjectionManager
    {
        private readonly HashSet<ArchiveListing> _set = new HashSet<ArchiveListing>();

        public UiInjectionManager()
        {
            //if (InteractionService.GamePart != FFXIIIGamePart.Part1)
            //    throw new NotSupportedException($"Injection to the Final Fantasty 13-{(Int32)InteractionService.GamePart} has not yet supported.");
        }

        public void Enqueue(ArchiveListing parent)
        {
            _set.Add(parent);
        }

        public void WriteListings()
        {
            HashSet<ArchiveListing> set = new HashSet<ArchiveListing>();
            foreach (ArchiveListing listing in _set)
            {
                ArchiveListing item = listing;
                while (item != null && set.Add(item))
                    item = item.Parent;
            }

            Action<ArchiveListing> writer;
            switch (InteractionService.GamePart)
            {
                case FFXIIIGamePart.Part1:
                    writer = ArchiveListingWriterV1.Write;
                    break;
                case FFXIIIGamePart.Part2:
                    writer = ArchiveListingWriterV2.Write;
                    break;
                default:
                    throw new NotSupportedException(InteractionService.GamePart.ToString());
            }

            foreach (ArchiveListing listing in set.OrderByDescending(l => l.Accessor.Level))
                writer(listing);
        }
    }
}