using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherInstallButton : UiProgressButton
    {
        private const string InstallLabel = "Установить";
        private const string InstallationLabel = "Установка...";

        public UiPatcherInstallButton()
        {
            Label = InstallLabel;
        }

        protected override async Task DoAction()
        {
            Label = InstallationLabel;
            try
            {
                using (SafeUnmanagedArray buff = await Decompress())
                {
                    if (CancelEvent.IsSet())
                        return;

                    using (Stream input = buff.OpenStream(FileAccess.Read))
                        await PatchAsync(input);
                }
            }
            finally
            {
                Label = InstallLabel;
            }
        }

        private void OnProgress(long value)
        {
            Position += value;
        }

        private async Task<SafeUnmanagedArray> Decompress()
        {
            string securityKey = await ((MainWindow)this.GetRootElement()).GetSecurityKeyAsync(false); // todo
            if (CancelEvent.IsSet())
                return null;

            using (FileStream input = File.OpenRead(PatcherService.ArchiveFileName))
            using (MemoryStream ms = new MemoryStream((int)input.Length))
            {
                if (CancelEvent.IsSet())
                    return null;

                Maximum = input.Length;
                using (CryptoProvider cryptoProvider = new CryptoProvider(securityKey, CancelEvent))
                {
                    cryptoProvider.Progress += OnProgress;
                    await cryptoProvider.Decrypt(input, ms);
                }

                if (CancelEvent.IsSet())
                    return null;

                Position = ms.Position = 0;
                BinaryReader br = new BinaryReader(ms);
                int uncompressedSize = br.ReadInt32();
                byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];

                if (CancelEvent.IsSet())
                    return null;

                SafeUnmanagedArray result = new SafeUnmanagedArray(uncompressedSize);
                try
                {
                    if (CancelEvent.IsSet())
                        return null;

                    Maximum = uncompressedSize;
                    using (UnmanagedMemoryStream output = result.OpenStream(FileAccess.Write))
                        ZLibHelper.Uncompress(ms, output, uncompressedSize, buff, CancellationToken.None, OnProgress);
                }
                catch
                {
                    result.SafeDispose();
                    throw;
                }

                return result;
            }
        }

        private async Task PatchAsync(Stream input)
        {
            await Task.Factory.StartNew(() => Patch(input));
        }

        private void Patch(Stream input)
        {
            Position = 0;
            Maximum = input.Length;

            if (CancelEvent.IsSet())
                return;

            BinaryReader br = new BinaryReader(input);

            PatchFormatVersion fileVersion = (PatchFormatVersion)input.ReadByte();
            if (fileVersion > PatcherService.Version)
                throw new NotSupportedException("Пожалуйста, обновите программу установки.");
            OnProgress(1);

            if (CancelEvent.IsSet())
                return;

            FFXIIIGamePart gamePart = (FFXIIIGamePart)input.ReadByte();
            InteractionService.SetGamePart(gamePart);
            OnProgress(1);

            if (CancelEvent.IsSet())
                return;

            GameLocationInfo gameLocation = PatcherService.GetGameLocation(gamePart);
            PatchText(br, gameLocation);
        }

        private void PatchText(BinaryReader br, GameLocationInfo gameLocation)
        {
            if (CancelEvent.IsSet())
                return;

            FFXIIITextEncoding encoding = ReadEncoding(br);
            TextEncodingInfo encodingInfo = new TextEncodingInfo(encoding);
            InteractionService.TextEncoding.SetValue(encodingInfo);
            if (CancelEvent.IsSet())
                return;

            //long position = br.BaseStream.Position;
            //XgrPatchData fonts = XgrPatchData.ReadFrom(br);
            //OnProgress(br.BaseStream.Position - position);
            //if (CancelEvent.IsSet())
            //    return;


            UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(gameLocation);
            using (MemoryInjectionAccessor source = new MemoryInjectionAccessor())
            {
                UiInjectionManager manager = new UiInjectionManager();

                long position = br.BaseStream.Position;
                using (XgrPatchData fonts = XgrPatchData.ReadFrom(br))
                {
                    OnProgress(br.BaseStream.Position - position);
                    if (CancelEvent.IsSet())
                        return;

                    foreach (KeyValuePair<string, SafeUnmanagedArray> data in fonts)
                        source.RegisterStream(Path.Combine(fonts.XgrArchiveUnpackName, data.Key), data.Value.OpenStream(FileAccess.Read));

                    Dictionary<string, string> dic = ReadStrings(br);
                    if (CancelEvent.IsSet())
                        return;

                    source.RegisterStrings(dic);
                    if (CancelEvent.IsSet())
                        return;

                    UiArchives archives = builder.Build();
                    Position = 0;
                    Maximum = archives.Count;
                    foreach (UiContainerNode archive in archives)
                    {
                        Check(archive);
                        OnProgress(1);
                    }
                    if (CancelEvent.IsSet())
                        return;

                    IUiLeafsAccessor[] accessors = archives.AccessToCheckedLeafs(new Wildcard("*"), null, false).ToArray();
                    Position = 0;
                    Maximum = accessors.Length;
                    foreach (IUiLeafsAccessor accessor in accessors)
                    {
                        accessor.Inject(source, manager);
                        OnProgress(1);
                    }
                }

                manager.WriteListings();
            }
        }

        private void Check(UiNode node)
        {
            switch (node.Type)
            {
                case UiNodeType.Group:
                case UiNodeType.Directory:
                case UiNodeType.Archive:
                {
                    foreach (UiNode child in node.GetChilds())
                        Check(child);
                    break;
                }
                case UiNodeType.DataTable:
                {
                    if (PathComparer.Instance.Value.Equals(node.Name, @"system.win32.xgr") || node.Name.StartsWith("tutorial"))
                        foreach (UiNode child in node.GetChilds())
                            Check(child);
                    break;
                }
                case UiNodeType.ArchiveLeaf:
                {
                    UiArchiveLeaf leaf = (UiArchiveLeaf)node;
                    string extension = PathEx.GetMultiDotComparableExtension(leaf.Entry.Name);
                    switch (extension)
                    {
                        case ".ztr":
                            if (leaf.Entry.Name.Contains("_us"))
                                leaf.IsChecked = true;
                            break;
                    }
                    break;
                }
                case UiNodeType.DataTableLeaf:
                {
                    UiWpdTableLeaf leaf = (UiWpdTableLeaf)node;
                    switch (leaf.Entry.Extension)
                    {
                        case "wfl":
                        case "txbh":
                            leaf.IsChecked = true;
                            break;
                    }
                    break;
                }
            }
        }

        private FFXIIITextEncoding ReadEncoding(BinaryReader br)
        {
            int length = br.ReadInt32();
            OnProgress(4);

            using (MemoryStream ms = new MemoryStream(length))
            {
                byte[] buff = new byte[4096];
                br.BaseStream.CopyToStream(ms, length, buff);
                ms.Position = 0;

                XmlDocument doc = new XmlDocument();
                doc.Load(ms);

                OnProgress(length);

                FFXIIICodePage codepage = FFXIIICodePageHelper.FromXml(doc.GetDocumentElement());
                return new FFXIIITextEncoding(codepage);
            }
        }

        private Dictionary<string, string> ReadStrings(BinaryReader br)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(16000);
            OnProgress(4);

            for (int count = br.ReadInt32(); count > 0; count = br.ReadInt32())
            {
                long position = br.BaseStream.Position;
                if (CancelEvent.IsSet())
                    return result;

                for (int i = 0; i < count; i++)
                {
                    string key = br.ReadString();
                    string value = br.ReadString();
                    result[key] = value;
                }
                OnProgress(br.BaseStream.Position - position);
                OnProgress(4);
            }

            return result;
        }
    }
}