using System.IO;
using System.Runtime.InteropServices;
using NAudio.Wave;
using Pulse.Core;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class WaveFormatEx : IStreamingContent
    {
        /// <summary>
        /// Waveform-audio format type. Format tags are registered with Microsoft Corporation for many compression algorithms. A complete list of format tags can be found in the Mmreg.h header file. For one- or two-channel PCM data, this value should be WAVE_FORMAT_PCM. When this structure is included in a WAVEFORMATEXTENSIBLE structure, this value must be WAVE_FORMAT_EXTENSIBLE.
        /// </summary>
        public WaveFormatEncoding FormatTag;

        /// <summary>
        /// Number of channels in the waveform-audio data. Monaural data uses one channel and stereo data uses two channels.
        /// </summary>
        public short Channels;

        /// <summary>
        /// Sample rate, in samples per second (hertz). If wFormatTag is WAVE_FORMAT_PCM, then common values for nSamplesPerSec are 8.0 kHz, 11.025 kHz, 22.05 kHz, and 44.1 kHz. For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.
        /// </summary>
        public int SamplesPerSec;

        /// <summary>
        /// Required average data-transfer rate, in bytes per second, for the format tag. If wFormatTag is WAVE_FORMAT_PCM, nAvgBytesPerSec should be equal to the product of nSamplesPerSec and nBlockAlign. For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.
        /// </summary>
        public int AverageBytesPerSecond;

        /// <summary>
        /// Block alignment, in bytes. The block alignment is the minimum atomic unit of data for the wFormatTag format type. If wFormatTag is WAVE_FORMAT_PCM or WAVE_FORMAT_EXTENSIBLE, nBlockAlign must be equal to the product of nChannels and wBitsPerSample divided by 8 (bits per byte). For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.
        /// Software must process a multiple of nBlockAlign bytes of data at a time. Data written to and read from a device must always start at the beginning of a block. For example, it is illegal to start playback of PCM data in the middle of a sample (that is, on a non-block-aligned boundary).
        /// </summary>
        public short BlockAlign;

        /// <summary>
        /// Bits per sample for the wFormatTag format type. If wFormatTag is WAVE_FORMAT_PCM, then wBitsPerSample should be equal to 8 or 16. For non-PCM formats, this member must be set according to the manufacturer's specification of the format tag. If wFormatTag is WAVE_FORMAT_EXTENSIBLE, this value can be any integer multiple of 8 and represents the container size, not necessarily the sample size; for example, a 20-bit sample size is in a 24-bit container. Some compression schemes cannot define a value for wBitsPerSample, so this member can be 0.
        /// </summary>
        public short BitsPerSample;

        /// <summary>
        /// Size, in bytes, of extra format information appended to the end of the WAVEFORMATEX structure. This information can be used by non-PCM formats to store extra attributes for the wFormatTag. If no extra information is required by the wFormatTag, this member must be set to 0. For WAVE_FORMAT_PCM formats (and only WAVE_FORMAT_PCM formats), this member is ignored. When this structure is included in a WAVEFORMATEXTENSIBLE structure, this value must be at least 22.
        /// </summary>
        public short ExtraDataSize;

        public byte[] ExtraData;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            FormatTag = (WaveFormatEncoding)br.ReadInt16();
            Channels = br.ReadInt16();
            SamplesPerSec = br.ReadInt32();
            AverageBytesPerSecond = br.ReadInt32();
            BlockAlign = br.ReadInt16();
            BitsPerSample = br.ReadInt16();
            ExtraDataSize = br.ReadInt16();
            ExtraData = stream.EnsureRead(ExtraDataSize);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write((short)FormatTag);
            bw.Write(Channels);
            bw.Write(SamplesPerSec);
            bw.Write(AverageBytesPerSecond);
            bw.Write(BlockAlign);
            bw.Write(BitsPerSample);
            bw.Write(ExtraDataSize);
            stream.Write(ExtraData, 0, ExtraDataSize);
        }
    }
}