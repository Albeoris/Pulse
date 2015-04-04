using System.Threading;

namespace Pulse.Core
{
    public static class ThreadHelper
    {
        public static Thread Start(string name, ThreadStart action)
        {
            Thread result = new Thread(action) {Name = name};
            result.Start();
            return result;
        }
    }
}