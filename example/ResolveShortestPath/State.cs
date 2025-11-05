namespace ResolveShortestPath;

public struct State(ulong node, double[] variables, int depth, double probability, LinkNode linkNode)
{
    public ulong Node = node;
    public double[] Variables = variables;
    public int Depth = depth;
    public double Probability = probability;
    public LinkNode LinkNode = linkNode;
}
