using System;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Pulse.UI
{
    public sealed class GameLocationUserProvider : IInfoProvider<GameLocationInfo>
    {
        public GameLocationInfo Provide()
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Укажите каталог Final Fantasy XIII..."))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                    throw new OperationCanceledException();

                GameLocationInfo result = new GameLocationInfo(dlg.FileName);
                result.Validate();

                return result;
            }
        }

        public string Title
        {
            get { return "Ручной выбор"; }
        }

        public string Description
        {
            get { return "Позволяет вручную выбрать каталог, в который была установлена игра."; }
        }
    }
}