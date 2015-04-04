namespace Pulse.UI
{
    public interface IUiLeaf
    {
        string Name { get; }
        UiNodeType Type { get; }
    }
}