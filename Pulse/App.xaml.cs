using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Message("Приложение запущено.");

            //// book.strings => review.strings
            //ZtrFileEntry[] books, reviews;
            //using (Stream book = File.OpenRead(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII\Pack\Strings\book.strings\ru-RU\book.strings"))
            //using (Stream review = File.OpenRead(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII\Pack\Strings\review.strings\ru-RU\review.strings"))
            //{
            //    string name;
            //    ZtrTextReader reader = new ZtrTextReader(book, StringsZtrFormatter.Instance);
            //    books = reader.Read(out name);
            //    reader = new ZtrTextReader(review, StringsZtrFormatter.Instance);
            //    reviews = reader.Read(out name);
            //}
            //
            //foreach (ZtrFileEntry entrie in reviews)
            //{
            //    string postfix = entrie.Key.Substring(6);
            //    Wildcard wc = new Wildcard("$atar_" + postfix + "p?");
            //    entrie.Value =
            //        String.Join("{Text NewLine}{Text NewLine}", books.Where((k, v) => wc.IsMatch(k.Key)).OrderBy(z => z.Key).Select(z => z.Value))
            //        .Replace("{Text 112}", String.Empty)
            //        .Replace("{Text NewPage}", String.Empty);
            //}
            //
            //Directory.CreateDirectory(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII\Pack\Strings\review.strings\ru-RU\");
            //
            //using (Stream output = File.Create(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII\Pack\Strings\review.strings\ru-RU\review.strings"))
            //{
            //    ZtrTextWriter wrieter = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
            //    wrieter.Write(@"D:\Temp\FFXIII\txtres\resident\review\txtres_us.ztr", reviews);
            //}
            //
            //Environment.Exit(1);


            UiMainWindow main = new UiMainWindow();
            UiGamePartSelectDialog dlg = new UiGamePartSelectDialog();
            if (dlg.ShowDialog() != true)
                Environment.Exit(1);

            InteractionService.SetGamePart(dlg.Result);
            main.Show();
        }
    }
}