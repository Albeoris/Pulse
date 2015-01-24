using System;
using System.Windows;
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
            UiMainWindow main = new UiMainWindow();
            UiGamePartSelectDialog dlg = new UiGamePartSelectDialog();
            if (dlg.ShowDialog() != true)
                Environment.Exit(1);
            
            InteractionService.SetGamePart(dlg.Result);
            main.Show();
        }
    }
}