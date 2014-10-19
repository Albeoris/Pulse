using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            new UiMainWindow().Show();
        }
    }
}