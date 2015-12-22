using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingReaderV3 : IDisposable
    {
        public static ArchiveListing Read(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            using (ArchiveListingReaderV3 reader = new ArchiveListingReaderV3(accessor, progressIncrement, progressTotalChanged))
            {
                ArchiveListing result = reader.Read();
                return result;
            }
        }

        private readonly ArchiveAccessor _accessor;
        private readonly Action<long> _progressIncrement;
        private readonly Action<long> _progressTotalChanged;

        private ArchiveListingReaderV3(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            _accessor = accessor;
            _progressIncrement = progressIncrement;
            _progressTotalChanged = progressTotalChanged;
        }

        public void Dispose()
        {
        }

        public ArchiveListing Read()
        {
            ArchiveListingHeaderV3 header;
            using (Stream input = Decrypt(out header))
            {

                _progressTotalChanged.NullSafeInvoke(header.EntriesCount);

                short blockNumber = -1;
                bool? flag = null;

                ArchiveListingEntryInfoV3[] entries = new ArchiveListingEntryInfoV3[header.EntriesCount];
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i] = input.ReadContent<ArchiveListingEntryInfoV3>();
                    if (entries[i].Flag != flag)
                    {
                        flag = entries[i].Flag;
                        blockNumber++;
                    }
                    entries[i].BlockNumber = blockNumber;
                }

                ArchiveListingCompressedData data = new ArchiveListingCompressedData(header);
                data.ReadFromStream(input);

                ArchiveListing result = new ArchiveListing(_accessor, header);
                ParseEntries(entries, data, result);
                return result;
            }
        }

        private Stream Decrypt(out ArchiveListingHeaderV3 header)
        {
            Stream result = _accessor.ExtractListing();
            try
            {
                header = result.ReadContent<ArchiveListingHeaderV3>();
                if (header.IsValid(result.Length))
                {
                    header.IsEncrypted = false;
                    return result;
                }

                /*using (*/
                TempFileProvider tmpProvider = new TempFileProvider(_accessor.ListingEntry.Name /*"filelist"*/, ".win32.bin");/*)*/
                {
                    using (Stream output = tmpProvider.Create())
                    {
                        output.WriteContent(header);
                        result.CopyTo(output);
                        result.SafeDispose();
                    }

                    Process decrypter = new Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = @"Resources\Executable\ffxiiicrypt.exe",
                            Arguments = "-d \"" + tmpProvider.FilePath + "\" 2",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };
                    decrypter.Start();
                    Task<string> erroMessage = decrypter.StandardError.ReadToEndAsync();
                    Task<string> outputMessage = decrypter.StandardOutput.ReadToEndAsync();
                    decrypter.WaitForExit();
                    if (decrypter.ExitCode != 0)
                    {
                        StringBuilder sb = new StringBuilder("Decryption error! Code: ");
                        sb.AppendLine(decrypter.ExitCode.ToString());
                        sb.AppendLine("Error: ");
                        sb.AppendLine(erroMessage.Result);
                        sb.AppendLine("Output: ");
                        sb.AppendLine(outputMessage.Result);

                        throw new InvalidDataException(sb.ToString());
                    }

                    result = tmpProvider.OpenRead();
                    header = result.ReadContent<ArchiveListingHeaderV3>();
                    header.IsEncrypted = true;
                    if (!header.IsValid(result.Length))
                        throw new InvalidDataException();
                }

                return result;
            }
            catch
            {
                result.SafeDispose();
                throw;
            }
        }

        private void ParseEntries(ArchiveListingEntryInfoV3[] entries, ArchiveListingCompressedData data, ArchiveListing result)
        {
            byte[] buff = new byte[0];

            for (int currentBlock = -1, i = 0; i < entries.Length; i++)
            {
                ArchiveListingEntryInfoV3 entryInfoV3 = entries[i];
                if (entryInfoV3.BlockNumber != currentBlock)
                {
                    currentBlock = entryInfoV3.BlockNumber;
                    buff = data.AcquireData(currentBlock);
                }

                string name;
                long sector, uncompressedSize, compressedSize;
                ParseInfo(entryInfoV3, buff, out sector, out uncompressedSize, out compressedSize, out name);

                ArchiveEntry entry = new ArchiveEntry(name, sector, compressedSize, uncompressedSize)
                {
                    UnknownNumber = entryInfoV3.UnknownNumber,
                    UnknownValue = entryInfoV3.UnknownValue,
                    UnknownData = entryInfoV3.UnknownData
                };

                result.Add(entry);
                _progressIncrement.NullSafeInvoke(1);
            }
        }

        private void ParseInfo(ArchiveListingEntryInfoV3 entryInfo, byte[] uncompressedData, out long sector, out long uncompressedSize, out long compressedSize, out string name)
        {
            string[] info;
            unsafe
            {
                fixed (byte* ptr = &uncompressedData[entryInfo.Offset])
                {
                    string str = new string((sbyte*)ptr);
                    info = str.Split(':');
                }
            }

            if (info.Length < 4)
            {
                name = String.Join(":", info);
                sector = -1;
                uncompressedSize = -1;
                compressedSize = -1;
            }
            else
            {
                sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                name = info[3];
            }
        }
    }
}