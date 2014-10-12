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

                string gamePath = provider.GamePath;
                Exceptions.CheckDirectoryNotFoundException(gamePath);
                
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