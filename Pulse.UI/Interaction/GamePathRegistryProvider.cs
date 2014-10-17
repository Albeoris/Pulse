using System.IO;
using Microsoft.Win32;

namespace Pulse.UI
{
    public sealed class GamePathRegistryProvider : IGamePathProvider
    {
        private const string SteamRegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120";
        private const string SteamGamePathTag = @"InstallLocation";


        public void Provide()
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(SteamRegistyPath))
            {
                GamePath = (string)registryKey.GetValue(SteamGamePathTag);
                GameDataPath = Path.Combine(GamePath, "white_data");
            }
        }

        public string GamePath { get; private set; }
        public string GameDataPath { get; private set; }
    }
}