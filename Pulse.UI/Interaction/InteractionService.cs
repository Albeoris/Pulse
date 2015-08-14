using System;
using System.Windows;
using Pulse.Core;
using Pulse.UI.Interaction;

namespace Pulse.UI
{
    public static class InteractionService
    {
        public static FFXIIIGamePart GamePart { get; private set; }
        public static ApplicationConfigProviders Configuration { get; private set; }
        public static AudioSettingsProviders AudioSettings { get; private set; }
        public static GameLocationProviders GameLocation { get; private set; }
        public static WorkingLocationProviders WorkingLocation { get; private set; }
        public static TextEncodingProviders TextEncoding { get; private set; }
        public static LocalizatorEnvironmentProviders LocalizatorEnvironment { get; private set; }

        public static event Action<IUiLeaf> SelectedLeafChanged;

        static InteractionService()
        {
            Configuration = new ApplicationConfigProviders();
            AudioSettings = new AudioSettingsProviders();
            GameLocation = new GameLocationProviders();
            WorkingLocation = new WorkingLocationProviders();
            TextEncoding = new TextEncodingProviders();
            LocalizatorEnvironment = new LocalizatorEnvironmentProviders();

            GameLocation.InfoProvided += Configuration.GameLocationProvided;
            WorkingLocation.InfoProvided += Configuration.WorkingLocationProvided;
            LocalizatorEnvironment.InfoProvided += Configuration.LocalizatorEnvironmentProvided;
        }

        public static void SetGamePart(FFXIIIGamePart result)
        {
            GamePart = result;
        }

        public static void RaiseSelectedNodeChanged(UiNode node)
        {
            try
            {
                SelectedLeafChanged.NullSafeInvoke(node as IUiLeaf);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(Application.Current.MainWindow, ex);
            }
        }
    }
}