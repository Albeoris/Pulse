namespace Pulse.UI
{
    public interface IGamePathProvider : IInfoProvider
    {
        string GamePath { get; }
        string GameDataPath { get; }
    }
}