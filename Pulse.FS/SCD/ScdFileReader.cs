using System;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ScdFileReader
    {
        private readonly Stream _input;

        public ScdFileReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
        }

        public WaveStream[] Read()
        {
            SectionHeader sectionHeader = _input.ReadContent<SectionHeader>();
            SscfHeader sscfHeader = _input.ReadContent<SscfHeader>();

            BinaryReader br = new BinaryReader(_input);
            _input.SetPosition(sscfHeader.WavesOffset);
            int[] offsets = new int[sscfHeader.NumWaves];
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = br.ReadInt32();

            WaveStream[] result = new WaveStream[sscfHeader.NumWaves];
            for (int i = 0; i < offsets.Length; i++)
            {
                _input.SetPosition(offsets[i]);
                SscfWaveHeader waveHeader = _input.ReadContent<SscfWaveHeader>();
                if (waveHeader.Format == SscfWaveFormat.Vorbis)
                {
                    _input.SetPosition(waveHeader.DataOffset);
                    byte[] vorbisData = _input.EnsureRead(waveHeader.DataLength);
                    MemoryStream vorbisMs = new MemoryStream(vorbisData, 0, vorbisData.Length, false);
                    result[i] = new VorbisWaveReader(vorbisMs);
                    continue;
                }

                WaveFormat format = ReadWaveFormat(waveHeader);
                if (format == null)
                    continue;


                _input.SetPosition(waveHeader.DataOffset);
                byte[] data = _input.EnsureRead(waveHeader.DataLength);
                MemoryStream ms = new MemoryStream(data, 0, data.Length, false);
                result[i] = new RawSourceWaveStream(ms, format);
            }
            return result;
        }

        private WaveFormat ReadWaveFormat(SscfWaveHeader waveHeader)
        {
            switch (waveHeader.Format)
            {
                case SscfWaveFormat.Empty:
                    if (waveHeader.DataLength != 0)
                        throw new InvalidDataException();
                    return null;
                case SscfWaveFormat.Pcm:
                    return CreatePcmWaveFormat(waveHeader);
                case SscfWaveFormat.Vorbis:
                    //voice = new VorbisWave() {Parent = this, WaveHeader = waveHeader};
                    throw new NotSupportedException("Unsupported Wave Format: " + waveHeader.Format);
                case SscfWaveFormat.MsAdPcm:
                    return ReadMicrosoftAdPcmWaveFormat(waveHeader);
                case SscfWaveFormat.Atrac3:
                case SscfWaveFormat.Atrac3Too:
                case SscfWaveFormat.Xma:
                    throw new NotSupportedException("Unsupported Wave Format: " + waveHeader.Format);
                default:
                    throw new NotImplementedException("Unknown Wave Format: 0x" + ((int)waveHeader.Format).ToString("X"));
            }
        }

        private WaveFormat CreatePcmWaveFormat(SscfWaveHeader waveHeader)
        {
            const int bitsPerSample = 16;
            short channels = (short)waveHeader.NumChannels;
            short blockAlign = (short)(channels * 2);
            int sampleRate = waveHeader.SamplingRate;
            int averageBytesPerSecond = sampleRate * blockAlign;

            return WaveFormat.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                sampleRate,
                channels,
                averageBytesPerSecond,
                blockAlign,
                bitsPerSample);
        }

        private WaveFormat ReadMicrosoftAdPcmWaveFormat(SscfWaveHeader waveHeader)
        {
            BinaryReader br = new BinaryReader(_input);
            return WaveFormat.FromFormatChunk(br, waveHeader.FormatHeaderLength);
        }
    }
}