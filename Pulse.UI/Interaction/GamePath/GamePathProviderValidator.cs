using System;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GamePathProviderValidator
    {
        public bool Validate(IGamePathProvider provider, out string error)
        {
            try
            {
                provider.Provide();

                Exceptions.CheckDirectoryNotFoundException(provider.GamePath);
                Exceptions.CheckDirectoryNotFoundException(provider.GameDataPath);
                
                error = null;
                return true;
            }
            catch(Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}