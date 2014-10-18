using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Pulse.UI
{
    public sealed class GamePathUserProvider : IGamePathProvider
    {
        public void Provide()
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Укажите каталог Final Fantasy XIII..."))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    GamePath = dlg.FileName;
                    GameDataPath = Path.Combine(GamePath, "white_data");
                }
            }
        }

        public string GamePath { get; private set; }
        public string GameDataPath { get; private set; }
    }
}