namespace Pulse.UI
{
    public interface IUiLeafsAccessor
    {
        UiNodeType Type { get; }
        void Extract(IUiExtractionTarget target);
        void Inject(IUiInjectionSource source, UiInjectionManager manager);
    }
}