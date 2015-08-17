using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingWriterV2
    {
        public static void Write(ArchiveListing listing)
        {
            ArchiveListingWriterV2 writer = new ArchiveListingWriterV2(listing);
            writer.Write();
        }

        private readonly ArchiveListing _listing;
        private readonly ArchiveAccessor _accessor;

        private ArchiveListingWriterV2(ArchiveListing listing)
        {
            _listing = listing;
            _accessor = _listing.Accessor;
        }

        public void Write()
        {
            using (MemoryStream headerBuff = new MemoryStream(32768))
            using (MemoryStream textBuff = new MemoryStream(32768))
            {
                ArchiveListingBlockInfo[] blocksInfo;
                ArchiveListingEntryInfoV2[] entriesInfoV2;
                ArchiveListingTextWriterV2 textWriter = new ArchiveListingTextWriterV2(textBuff);
                textWriter.Write(_listing, out blocksInfo, out entriesInfoV2);

                for (int i = 0; i < entriesInfoV2.Length; i++)
                {
                    ArchiveListingEntryInfoV2 info = entriesInfoV2[i];
                    ArchiveEntry entry = _listing[i];
                    info.UnknownNumber = entry.UnknownNumber;
                    info.UnknownValue = entry.UnknownValue;
                    info.UnknownData = entry.UnknownData;
                }

                byte[] buff = new byte[8192];
                int blocksSize = (int)textBuff.Position;
                textBuff.Position = 0;

                ArchiveListingHeaderV2 header = (ArchiveListingHeaderV2)_listing.Header;
                header.EntriesCount = entriesInfoV2.Length;
                header.RawBlockOffset = entriesInfoV2.Length * 8 + 12;
                header.RawInfoOffset = header.RawBlockOffset + blocksInfo.Length * 12;

                headerBuff.WriteContent(header);
                foreach (ArchiveListingEntryInfoV2 entry in entriesInfoV2)
                    headerBuff.WriteContent(entry);
                foreach (ArchiveListingBlockInfo block in blocksInfo)
                    headerBuff.WriteStruct(block);

                int hederSize = (int)headerBuff.Length;
                headerBuff.Position = 0;

                if (header.IsEncrypted)
                {
                    RecreateEncryptedListing(headerBuff, hederSize, textBuff, blocksSize, buff);
                }
                else
                {
                    using (Stream output = _accessor.RecreateListing(hederSize + blocksSize))
                    {
                        headerBuff.CopyToStream(output, hederSize, buff);
                        textBuff.CopyToStream(output, blocksSize, buff);
                    }
                }
            }
        }

        private void RecreateEncryptedListing(MemoryStream headerBuff, int hederSize, MemoryStream textBuff, int blocksSize, byte[] buff)
        {
            using (TempFileProvider tmpProvider = new TempFileProvider("filelist", ".win32.bin"))
            {
                using (Stream output = tmpProvider.Create())
                {
                    headerBuff.CopyToStream(output, hederSize, buff);
                    textBuff.CopyToStream(output, blocksSize, buff);
                }

                Process encrypter = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"Resources\Executable\ffxiiicrypt.exe",
                        Arguments = "-e \"" + tmpProvider.FilePath + "\" 2",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                encrypter.Start();
                Task<string> erroMessage = encrypter.StandardError.ReadToEndAsync();
                Task<string> outputMessage = encrypter.StandardOutput.ReadToEndAsync();
                encrypter.WaitForExit();
                if (encrypter.ExitCode != 0)
                {
                    StringBuilder sb = new StringBuilder("Decryption error! Code: ");
                    sb.AppendLine(encrypter.ExitCode.ToString());
                    sb.AppendLine("Error: ");
                    sb.AppendLine(erroMessage.Result);
                    sb.AppendLine("Output: ");
                    sb.AppendLine(outputMessage.Result);

                    throw new InvalidDataException(sb.ToString());
                }

                using (Stream input = tmpProvider.OpenRead())
                using (Stream output = _accessor.RecreateListing((Int32)input.Length))
                    input.CopyToStream(output, (Int32)input.Length, buff);
            }
        }
    }
}