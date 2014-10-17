using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Pulse.UI
{
    public sealed class InteractionService
    {
        public static readonly Dispatcher Dispatcher = Application.Current.MainWindow.Dispatcher;

        public static string GamePath { get; private set; }
        public static string GameDataPath { get; private set; }
        public static event Action Refreshed;

        public static void Refresh()
        {
            RefreshGamePath();

            Action h = Refreshed;
            if (h != null)
                Refreshed();
        }

        private static void RefreshGamePath()
        {
            IGamePathProvider[] providers = {new GamePathRegistryProvider(), new GamePathUserProvider()};
            GamePathProviderValidator validator = new GamePathProviderValidator();
            foreach (IGamePathProvider provider in providers)
            {
                string error;
                if (!validator.Validate(provider, out error))
                    continue;

                GamePath = provider.GamePath;
                GameDataPath = provider.GameDataPath;
                return;
            }
        }
    }
}