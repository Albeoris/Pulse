using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationUserProvider : IInfoProvider<GameLocationInfo>
    {
        public GameLocationInfo Provide()
        {
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
                return dispatcher.Invoke(() => Provide());

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
            get { return Lang.InfoProvider.GameLocation.UserTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.GameLocation.UserDescription; }
        }
    }
}