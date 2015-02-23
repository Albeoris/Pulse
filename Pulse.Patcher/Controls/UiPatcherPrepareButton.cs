using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherPrepareButton : UiProgressButton
    {
        private const string PrepareLabel = "Подготовить";
        private const string PreparationLabel = "Подготовка...";

        public UiPatcherPrepareButton()
        {
            Label = PrepareLabel;
        }

        protected override async Task DoAction()
        {
            Label = PreparationLabel;
            try
            {
                string securityKey = await ((MainWindow)this.GetRootElement()).GetSecurityKeyAsync(false);
                if (CancelEvent.IsSet())
                    return;

                string root, targetPath;
                using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Выберите каталог..."))
                {
                    dlg.IsFolderPicker = true;
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    root = dlg.FileName;
                    if (CancelEvent.IsSet())
                        return;
                }

                using (CommonSaveFileDialog dlg = new CommonSaveFileDialog("Сохранить как..."))
                {
                    dlg.DefaultFileName = "FF13TranslationAlpha.ecp";
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    targetPath = dlg.FileName;
                    if (CancelEvent.IsSet())
                        return;
                }

                await Task.Factory.StartNew(() => Pack(root, securityKey, targetPath));

            }
            finally
            {
                Label = PrepareLabel;
            }
        }

        private async Task Pack(string root, string securityKey, string targetPath)
        {
            if (CancelEvent.IsSet())
                return;

            using (MemoryStream buff = new MemoryStream(16 * 1024 * 1024))
            {
                BinaryWriter bw = new BinaryWriter(buff);
                bw.Write((byte)PatchFormatVersion.V1);
                bw.Write((byte)FFXIIIGamePart.Part1);
                if (CancelEvent.IsSet())
                    return;

                await PackTexts(root, bw);
                if (CancelEvent.IsSet())
                    return;

                buff.Flush();
                int length = (int)buff.Position;
                buff.Position = 0;

                using (MemoryStream compressed = new MemoryStream(length + 4 + 256))
                using (BinaryWriter cbw = new BinaryWriter(compressed))
                {
                    if (CancelEvent.IsSet())
                        return;

                    cbw.Write(length);
                    Position = 0;
                    Maximum = length;
                    ZLibHelper.Compress(buff, compressed, length, OnProgress);
                    compressed.Position = 0;
                    if (CancelEvent.IsSet())
                        return;

                    Position = 0;
                    Maximum = compressed.Length;
                    using (CryptoProvider cryptoProvider = new CryptoProvider(securityKey, CancelEvent))
                    {
                        cryptoProvider.Progress += OnProgress;

                        using (FileStream output = File.Create(targetPath))
                            await cryptoProvider.Encrypt(compressed, output);
                    }
                }
            }
        }

        private void OnProgress(long value)
        {
            Position += value;
        }

        private async Task PackTexts(string root, BinaryWriter bw)
        {
            await PackEncoding(root, bw);
            PackFonts(root, bw);
            PackStrings(root, bw);
        }

        private async Task PackEncoding(string root, BinaryWriter bw)
        {
            string path = Path.Combine(root, "TextEncoding.xml");

            using (FileStream input = File.OpenRead(path))
            {
                Position = 0;
                Maximum = input.Position;
                bw.Write((int)input.Length);
                await PatcherService.CopyAsync(input, bw.BaseStream, CancelEvent, OnProgress);
            }
        }

        private static void PackFonts(string root, BinaryWriter bw)
        {
            const string xgrArchiveName = @"gui/resident/system.win32.xgr";
            root = Path.Combine(root, @"Fonts");
            XgrPatchData.WriteTo(xgrArchiveName, root, bw);
        }

        private void PackStrings(string root, BinaryWriter bw)
        {
            if (CancelEvent.IsSet())
                return;

            root = Path.Combine(root, @"Strings");
            string[] files = Directory.GetFiles(root, "*.strings", SearchOption.AllDirectories);

            Position = 0;
            Maximum = files.Length;
            foreach (string file in files)
            {
                if (CancelEvent.IsSet())
                    return;

                using (FileStream input = File.OpenRead(file))
                {
                    string name;
                    ZtrTextReader reader = new ZtrTextReader(input, StringsZtrFormatter.Instance);
                    ZtrFileEntry[] entries = reader.Read(out name);
                    bw.Write(entries.Length);
                    foreach (ZtrFileEntry entry in entries)
                    {
                        bw.Write(entry.Key);
                        bw.Write(entry.Value);
                    }
                }
                OnProgress(1);
            }

            bw.Write(0);
        }
    }
}