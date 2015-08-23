using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
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

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            //using (Stream book = File.OpenRead(@"D:\Steam\SteamApps\common\FINAL FANTASY XIII\Work\Extracted\db\resident\movie_items_us.win32.wdb"))
            //{
            //    WdbMovieHeader header = book.ReadContent<WdbMovieHeader>();
            //}

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

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error((Exception)e.ExceptionObject, "Unexpected error");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                UiHelper.ShowError(null, e.Exception, "Unexpected error");
                e.Handled = true;
            }
            catch
            {
            }
        }
    }
}

