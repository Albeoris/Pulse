using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingProviders : InfoProviderGroup<TextEncodingInfo>
    {
        public TextEncodingProviders()
            :base(Lang.InfoProvider.TextEncoding.Title, Lang.InfoProvider.TextEncoding.Description)
        {
            Capacity = 3;

            TextEncodingUserProvider userProvider = new TextEncodingUserProvider();
            InfoProvided += userProvider.EncodingProvided;

            Add(new TextEncodingWorkingLocationProvider());
            Add(new TextEncodingNewProvider());
            Add(userProvider);
        }
    }
}