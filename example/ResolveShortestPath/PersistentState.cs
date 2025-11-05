namespace ResolveShortestPath;

public sealed class PersistentState(int depth, double probability)
    : IComparable<PersistentState>, IComparable<State>, IComparable<CidState>
{
    public int Depth = depth;
    public double Probability = probability;
    public LinkNode? Path = null;

    public int CompareTo(PersistentState? other)
    {
        if (other is null)
            return -1;
        int tmp = Depth.CompareTo(other.Depth);
        if (tmp != 0)
            return tmp;
        return other.Probability.CompareTo(Probability);
    }
    public int CompareTo(State other)
    {
        int tmp = Depth.CompareTo(other.Depth);
        if (tmp != 0)
            return tmp;
        return other.Probability.CompareTo(Probability);
    }
    public int CompareTo(CidState other)
    {
        int tmp = Depth.CompareTo(other.Depth);
        if (tmp != 0)
            return tmp;
        return other.Probability.CompareTo(Probability);
    }
    public override string ToString()
    {
        return $"De:{Depth};Pr:{Probability}";
    }
}