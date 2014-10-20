using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //foreach (var file in Directory.GetFiles(@"D:\Temp\FFXIII", "*.ztr", SearchOption.AllDirectories))
            //{
            //    using (var input = File.OpenRead(file))
            //    using (var output = File.Create(file + ".txt"))
            //    {
            //        ZtrFileUnpacker unpacker = new ZtrFileUnpacker(input);
            //        ZtrFileEntry[] entries = unpacker.Unpack();
            //
            //        ZtrTextWriter writer = new ZtrTextWriter(output);
            //        writer.Write(file, entries);
            //    }
            //}
            //new MainWindow().Show();

            //const string sourceListing = @"D:\Steam\SteamApps\common\FINAL FANTASY XIII\white_data\sys\filelistc.win32 - копия.bin";
            //const string sourceBinary = @"D:\Steam\SteamApps\common\FINAL FANTASY XIII\white_data\sys\white_imgc.win32.bin";

            //const string targetListing = @"D:\Steam\SteamApps\common\FINAL FANTASY XIII\white_data\sys\filelistc.win32.bin";
            //const string targetBinary = @"D:\Steam\SteamApps\common\FINAL FANTASY XIII\white_data\sys\white_imgc.win32.bin";

            //ArchiveAccessor sourceAccessor = new ArchiveAccessor(sourceBinary, sourceListing);
            //ArchiveListing listing = ArchiveListingReader.Read(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII", sourceAccessor).First(l => l.Name == "filelistc.win32 - копия.bin");


            //using (FileStream input = File.OpenRead(@"D:\Temp\FFXIII\txtres\event\ev_comn_000\txtres_us.ztr.txt"))
            //using (FileStream output = File.Create(@"D:\Temp\FFXIII\txtres\event\ev_comn_000\txtres_us.ztr.new"))
            //{
            //    string name;
            //    ZtrTextReader reader = new ZtrTextReader(input);
            //    ZtrFileEntry[] entries = reader.Read(out name);
            //
            //    ZtrFilePacker packer = new ZtrFilePacker(output);
            //    packer.Pack(entries);
            //}

            //foreach (var path in new[]
            //{
            //    "gui/resident/system.win32.imgb",
            //    "gui/resident/system.win32.xgr"
            //})
            //{
            //    ArchiveListingEntry entry = listing.First(n => n.Name.EndsWith(path));

            //    int compressedSize;
            //    using (Stream input = File.OpenRead(Path.Combine(@"D:\Temp\ZAMENA", path)))
            //    using (Stream buff = ZLibHelper.ReplaceEntryContent(input, entry, out compressedSize))
            //    using (Stream output = sourceAccessor.OpenOrAppendBinary(entry, compressedSize))
            //    {
            //        byte[] copyBuff = new byte[4096];
            //        buff.CopyTo(output, compressedSize, copyBuff);
            //    }
            //}
            //ArchiveListingEntry entry1 = listing.First(n => n.Name.EndsWith(@"ev_comn_000/txtres_us.ztr"));

            //int compressedSize1;
            //using (Stream input = File.OpenRead(@"D:\Temp\FFXIII\txtres\event\ev_comn_000\txtres_us.ztr.new"))
            //using (Stream buff = ZLibHelper.ReplaceEntryContent(input, entry1, out compressedSize1))
            //using (Stream output = sourceAccessor.OpenOrAppendBinary(entry1, compressedSize1))
            //{
            //    byte[] copyBuff = new byte[4096];
            //    buff.CopyTo(output, compressedSize1, copyBuff);
            //}
            
            //ArchiveAccessor targetAccessor = new ArchiveAccessor(targetBinary, targetListing);
            //ArchiveListingWriter.Write(listing, targetAccessor);

            new UiMainWindow().Show();
        }
    }
}