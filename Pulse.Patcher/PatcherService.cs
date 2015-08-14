using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public static class PatcherService
    {
        public const PatchFormatVersion Version = PatchFormatVersion.V1;
        public const string ArchiveFileName = "translation.zip";
        public const string ConfigurationFileName = "config.xml";

        private static readonly ConcurrentDictionary<FrameworkElement, FrameworkElement> Controls = new ConcurrentDictionary<FrameworkElement, FrameworkElement>();

        public static void RegisterControl(FrameworkElement element)
        {
            Controls.TryAdd(element, element);
        }

        public static void UnregisterControl(FrameworkElement element)
        {
            Controls.TryRemove(element, out element);
        }

        public static void ChangeEnableState(FrameworkElement excludedControll, bool state)
        {
            foreach (FrameworkElement control in Controls.Values)
            {
                if (!Equals(control, excludedControll))
                    control.IsEnabled = state;
            }
        }

        public static GameLocationInfo GetGameLocation(FFXIIIGamePart gamePart)
        {
            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                using (RegistryKey registryKey = localMachine.OpenSubKey(GetSteamRegistyPath(gamePart)))
                {
                    if (registryKey == null)
                        throw Exceptions.CreateException("Запись в реестре не обнаружена.");

                    GameLocationInfo result = new GameLocationInfo((string)registryKey.GetValue(GameLocationSteamRegistryProvider.SteamGamePathTag));
                    result.Validate();

                    return result;
                }
            }
            catch
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    using (CommonOpenFileDialog dlg = new CommonOpenFileDialog(String.Format("Укажите каталог Final Fantasy XIII-{0}...", (int)gamePart)))
                    {
                        dlg.IsFolderPicker = true;
                        if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                            throw new OperationCanceledException();

                        GameLocationInfo result = new GameLocationInfo(dlg.FileName);
                        result.Validate();

                        return result;
                    }
                });
            }
        }

        private static string GetSteamRegistyPath(FFXIIIGamePart gamePart)
        {
            switch (gamePart)
            {
                case FFXIIIGamePart.Part1:
                    return GameLocationSteamRegistryProvider.SteamRegistyPart1Path;
                case FFXIIIGamePart.Part2:
                    return GameLocationSteamRegistryProvider.SteamRegistyPart2Path;
                default:
                    throw new NotImplementedException();
            }
        }

        public static async Task CopyAsync(Stream input, Stream output, ManualResetEvent cancelEvent, Action<long> progress)
        {
            byte[] buff = new byte[32 * 1024];

            int read;
            while ((read = await input.ReadAsync(buff, 0, buff.Length)) != 0)
            {
                if (cancelEvent.WaitOne(0))
                    return;

                await output.WriteAsync(buff, 0, read);
                progress.NullSafeInvoke(read);
            }
        }
    }
}