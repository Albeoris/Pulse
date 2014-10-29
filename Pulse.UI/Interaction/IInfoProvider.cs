using System.Threading;

namespace Pulse.UI
{
    public interface IInfoProvider<out T> where T : class
    {
        string Title { get; }
        string Description { get; }

        T Provide();
    }
}