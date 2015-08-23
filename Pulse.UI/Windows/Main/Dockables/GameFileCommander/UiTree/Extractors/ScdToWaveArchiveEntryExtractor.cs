using System;
using System.IO;
using NAudio.Wave;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ScdToWaveArchiveEntryExtractor : IArchiveEntryExtractor
    {
        public string TargetExtension
        {
            get { return ".wav"; }
        }

        public void Extract(ArchiveEntry entry, StreamSequence output, Stream input, Byte[] buff)
        {
            int size = (int)entry.UncompressedSize;
            if (size == 0)
                return;

            ScdFileReader reader = new ScdFileReader(input);
            WaveStream[] waveStreams = reader.Read();
            if (waveStreams.Length == 0)
                return;

            Extract(output, waveStreams[0], buff);

            for (int i = 1; i < waveStreams.Length; i++)
            {
                if (!output.TryCreateNextStream(i.ToString("D3")))
                    throw new InvalidDataException();

                Extract(output, waveStreams[i], buff);
            }
        }

        private static void Extract(Stream output, WaveStream waveStream, byte[] buffer)
        {
            if (waveStream == null)
                return;

            using (WaveFileWriter writer = new WaveFileWriter(output, waveStream.WaveFormat))
            {
                int count;
                while ((count = waveStream.Read(buffer, 0, buffer.Length)) != 0)
                    writer.Write(buffer, 0, count);
            }
        }
    }
}