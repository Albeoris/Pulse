using System;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            get { return "Ручной выбор"; }
        }

        public string Description
        {
            get { return "Позволяет вручную выбрать рабочий каталог."; }
        }
    }
}