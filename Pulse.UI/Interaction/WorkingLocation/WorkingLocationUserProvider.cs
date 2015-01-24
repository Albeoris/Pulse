using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationUserProvider : IInfoProvider<WorkingLocationInfo>
    {
        public WorkingLocationInfo Provide()
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Укажите рабочий каталог..."))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                    throw new OperationCanceledException();

                WorkingLocationInfo result = new WorkingLocationInfo(dlg.FileName);
                result.Validate();

                return result;
            }
        }

        public string Title
        {
            get { return Lang.InfoProvider.WorkingLocation.UserTitle; }
        }

        public string Description
        {
            get { return Lang.InfoProvider.WorkingLocation.UserDescription; }
        }
    }
}