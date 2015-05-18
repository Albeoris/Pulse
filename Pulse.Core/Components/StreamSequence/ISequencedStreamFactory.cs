using System;
using System.IO;

namespace Pulse.Core
{
    public interface ISequencedStreamFactory
    {
        Boolean TryCreateNextStream(String key, out Stream result, out Exception ex);
    }
}