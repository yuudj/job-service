namespace JobService.Components
{
    public interface ConvertVideo
    {
        string GroupId { get; }
        int Index { get; }
        int Count { get; }
        string Path { get; }
    }
}