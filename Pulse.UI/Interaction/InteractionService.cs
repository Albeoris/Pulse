using System.Collections;
using System.Windows;
using System.Windows.Threading;
using Pulse.UI.Interaction;

namespace Pulse.UI
{
    public sealed class InteractionService
    {
        public static ApplicationConfigProviders Configuration { get; private set; }
        public static GameLocationProviders GameLocation { get; private set; }
        public static WorkingLocationProviders WorkingLocation { get; private set; }
        public static TextEncodingProviders TextEncoding { get; private set; }

        static InteractionService()
        {
            Configuration = new ApplicationConfigProviders();
            GameLocation = new GameLocationProviders();
            WorkingLocation = new WorkingLocationProviders();
            TextEncoding = new TextEncodingProviders();

            GameLocation.InfoProvided += Configuration.GameLocationProvided;
            WorkingLocation.InfoProvided += Configuration.WorkingLocationProvided;
        }
    }
}