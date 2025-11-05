namespace ResolveShortestPath;

public sealed class LinkNode(int index, ulong id, LinkNode? previous = null)
{
    public readonly int Index = index;
    public readonly ulong Id = id;
    public readonly LinkNode? Previous = previous;
}
