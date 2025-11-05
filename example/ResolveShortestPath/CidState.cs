namespace ResolveShortestPath;

public struct CidState(ulong cid, ulong node, double[] variables, int depth, double probability, LinkNode linkNode)
{
    public ulong Cid = cid;
    public ulong Node = node;
    public double[] Variables = variables;
    public int Depth = depth;
    public double Probability = probability;
    public LinkNode LinkNode = linkNode;
}
