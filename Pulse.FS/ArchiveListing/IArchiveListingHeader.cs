namespace Pulse.FS
{
    public interface IArchiveListingHeader
    {
        int BlockOffset { get; }
        int InfoOffset { get; }
        int EntriesCount { get; }
        int BlocksCount { get; }
    }
}