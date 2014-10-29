using System.IO;
using Microsoft.Win32;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationSteamRegistryProvider : IInfoProvider<GameLocationInfo>
    {
        private const string SteamRegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120";
        private const string SteamGamePathTag = @"InstallLocation";

        public GameLocationInfo Provide()
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(SteamRegistyPath))
            {
                if (registryKey == null)
                    throw Exceptions.CreateException("Запись в реестре не обнаружена.");

                GameLocationInfo result = new GameLocationInfo((string)registryKey.GetValue(SteamGamePathTag));
                result.Validate();
                
                return result;
            }
        }

        public string Title
        {
            get { return "Из реестр (Steam)"; }
        }

        public string Description
        {
            get { return "Получает информацию из реестра Windows, используя данные Steam."; }
        }
    }
}