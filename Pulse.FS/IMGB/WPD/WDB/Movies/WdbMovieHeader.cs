using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WdbMovieHeader : WdbHeader
    {
        public WdbMovieEntry[] Movies;

        public override void ReadFromStream(Stream input)
        {
            base.ReadFromStream(input);
            new Deserializer(this, input).Deserialize();
        }

        private sealed class Deserializer
        {
            private readonly WdbMovieHeader _header;
            private readonly Stream _input;

            public Deserializer(WdbMovieHeader header, Stream input)
            {
                _header = header;
                _input = input;
            }

            public void Deserialize()
            {
                if (_header.Entries == null)
                    return;

                int entryCount = Math.Max(0, _header.Entries.Length - SpecialEntriesCount);
                WdbMovieEntry[] entries = new WdbMovieEntry[entryCount];

                for (int i = 0; i < entryCount; i++)
                {
                    WpdEntry entry = _header.Entries[i + SpecialEntriesCount];
                    _input.SetPosition(entry.Offset);

                    if (entry.Length != WdbMovieEntry.StructSize)
                        throw new InvalidDataException($"[WdbMovieHeader.Deserialize] Entry: {entry.Name}, Length: {entry.Length}, Expected length: {WdbMovieEntry.StructSize}");

                    WdbMovieEntry movieEntry = _input.ReadContent<WdbMovieEntry>();
                    movieEntry.Entry = entry;
                    movieEntry.PackageName = _header.GetString(movieEntry.PackageNameOffset);
                    entries[i] = movieEntry;
                }

                _header.Movies = entries;
            }
        }
    }
}