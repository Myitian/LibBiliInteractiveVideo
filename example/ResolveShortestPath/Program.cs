using LibBiliInteractiveVideo;
using LibBiliInteractiveVideo.API;
using LibBiliInteractiveVideo.Execution;
using LibBiliInteractiveVideo.Execution.Compilation;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ResolveShortestPath;

class Program
{
    static async Task Main(string[] args)
    {
#if DEBUG
        XWebInterfaceViewDetail.RequestReady += Console.WriteLine;
        XWebInterfaceViewDetail.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XWebInterfaceViewDetail.json", json);
        XPlayerV2.RequestReady += Console.WriteLine;
        XPlayerV2.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XPlayerV2.json", json);
        XSteinEdgeinfoV2.RequestReady += Console.WriteLine;
        XSteinEdgeinfoV2.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XSteinEdgeinfoV2.json", json);
#endif
        using HttpClient httpClient = new();
        if (args.Length == 0)
        {
            while (true)
            {
                Console.WriteLine("AID/BVID:");
                string? line = Console.ReadLine();
                if (line is null)
                    break;
                await ProcessId(httpClient, line);
                Console.WriteLine();
            }
        }
        else
        {
            foreach (string line in args)
            {
                await ProcessId(httpClient, line);
                Console.WriteLine();
            }
        }
    }
    static double CalculateRandomProbability(scoped ReadOnlySpan<char> condition, VariableHolder<double> variables)
    {
        Dictionary<string, List<Condition<double>>> dict = [];
        var lookup = dict.GetAlternateLookup<ReadOnlySpan<char>>();
        foreach (NamedCondition<double> cond in new NamedConditionEnumerator<double>(condition))
        {
            if (!variables.ExtraInfo[variables.GetVariableIndex(cond.Name)].IsRandom)
                continue;
            if (lookup.TryGetValue(cond.Name, out List<Condition<double>>? list))
                list.Add(cond.Condition);
            else
                lookup[cond.Name] = [cond.Condition];
        }
        double finalProbability = 1;
        foreach ((_, List<Condition<double>> list) in dict)
        {
            list.Add(new(ConditionOperation.GE, 1));
            list.Add(new(ConditionOperation.LE, 100));
            (bool status, Condition<double>? condition1, Condition<double>? condition2) = Condition<double>.SimplifyAnds(list);
            if (status == false)
                return 0; // FALSE
            if (!condition1.HasValue)
                continue; // TRUE
            if (!condition2.HasValue)
                finalProbability *= 0.01; // EQ any(1~100)
            else
                finalProbability *= CountIntegersInRange(
                    (condition1.Value.Value, condition1.Value.Op == ConditionOperation.GE),
                    (condition2.Value.Value, condition2.Value.Op == ConditionOperation.LE)) * 0.01;
        }
        return finalProbability;

        static int CountIntegersInRange((double Value, bool Inclusive) p1, (double Value, bool Inclusive) p2)
        {
            int minInt = p1.Inclusive ? (int)Math.Ceiling(p1.Value) : (int)Math.Floor(p1.Value) + 1;
            int maxInt = p2.Inclusive ? (int)Math.Floor(p2.Value) : (int)Math.Ceiling(p2.Value) - 1;
            return minInt > maxInt ? 0 : maxInt - minInt + 1;
        }
    }

    static async Task ProcessId(HttpClient httpClient, string id)
    {
        (ulong graphVersion, ulong aid, _) = await VideoUtility.GetGraphVersion(httpClient, id);
        Dictionary<ulong, string> names = [];
        List<XSteinEdgeinfoV2.Data> edges = [];
        await foreach ((XSteinEdgeinfoV2.Data edge, _) in VideoUtility.ResolveAllEdges(httpClient, graphVersion, aid: aid))
        {
            names[edge.EdgeId] = edge.Title?.ReplaceLineEndings("") ?? "";
            edges.Add(edge);
        }
        InteractiveVideo<double> video = InteractiveVideo.ConvertFromAPI(edges);
        FrozenDictionary<ulong, PersistentState> result = ResolveShortestPath(video);
        Console.WriteLine($"NODE:{result.Count}");
        List<LinkNode> seq = [];
        FrozenDictionary<string, string> varDisplayNames = video.Variables.ExtraInfo
            .Select(it => new KeyValuePair<string, string>(it.Id, it.Name ?? ""))
            .ToFrozenDictionary();
        var lookup = varDisplayNames.GetAlternateLookup<ReadOnlySpan<char>>();
        ulong? trace = null;
#if DEBUG
        if (id == "BV1bhspzoENJ") trace = 43817460;
#endif
        foreach ((ulong node, PersistentState state) in result)
        {
            LinkNode? n;
            if (trace != node)
            {
                if (!trace.HasValue)
                {
                    Console.WriteLine($"{node}:");
                    Console.WriteLine($"De:{state.Depth};Pr:{state.Probability}");
                    n = state.Path;
                    while (n is not null)
                    {
                        Console.Write($"<{n.Id}");
                        n = n.Previous;
                    }
                    Console.WriteLine();
                }
                continue;
            }
            video.Variables.Reset();
            seq.Clear();
            Console.WriteLine($"{node}:");
            Console.WriteLine($"De:{state.Depth};Pr:{state.Probability}");
            n = state.Path;
            while (n is not null)
            {
                seq.Add(n);
                n = n.Previous;
            }
            ulong prev = 0;
            for (int i = seq.Count; i-- > 0;)
            {
                Console.WriteLine();
                LinkNode nn = seq[i];
                if (nn.Index >= 0)
                {
                    Edge<double> edge = video.Nodes[prev][nn.Index];
                    Console.WriteLine($"==>{edge.Option}");
                    bool first = true;
                    foreach (NamedCondition<double> cond in new NamedConditionEnumerator<double>(edge.RawCondition))
                    {
                        if (!first)
                            Console.Write(" &&");
                        else
                        {
                            Console.Write("  C:");
                            first = false;
                        }
                        Console.Write($" {lookup[cond.Name]} {cond.Condition.Op switch
                        {
                            ConditionOperation.EQ => "==",
                            ConditionOperation.LE => "<=",
                            ConditionOperation.LT => "<",
                            ConditionOperation.GE => ">=",
                            _ => ">",
                        }} {cond.Condition.Value}");
                    }
                    if (!edge.CheckWithoutRandom(video.Variables.Values))
                        Console.Write(" *** ERROR!");
                    if (!first)
                        Console.WriteLine();
                    first = true;
                    foreach (NamedNativeAction<double> action in new NamedNativeActionEnumerator<double>(edge.RawNativeAction))
                    {
                        if (first)
                        {
                            Console.Write("  N:");
                            first = false;
                        }
                        Console.Write($" {lookup[action.Store]} {action.NativeAction.Op switch
                        {
                            NativeActionOperation.Add => "+=",
                            NativeActionOperation.Subtract => "-=",
                            _ => "=",
                        }} {action.NativeAction.Value};");
                    }
                    if (!first)
                        Console.WriteLine();
                    edge.PerformAction(video.Variables.Values);
                }
                Console.WriteLine($"{prev = nn.Id}:{names[nn.Id]}");
                Console.WriteLine($"Va:[{string.Join(',', video.Variables.ExtraInfo
                    .Where(it => !it.IsRandom)
                    .Select(it => $"{it.Name}:{video.Variables.Values[it.ValueIndex]}"))}]");
            }
        }
    }
    static FrozenDictionary<ulong, PersistentState> ResolveShortestPath(InteractiveVideo<double> video, int depthLimit = 100)
    {
        if (video.Nodes.Count == 0)
            return FrozenDictionary<ulong, PersistentState>.Empty;
        FrozenDictionary<ulong, PersistentState> best = video.Nodes
            .Select(it => new KeyValuePair<ulong, PersistentState>(it.Key, new(int.MaxValue, 0)))
            .ToFrozenDictionary();
        FrozenDictionary<ulong, HashSet<double[]>> visited = video.Nodes
            .Select(it => new KeyValuePair<ulong, HashSet<double[]>>(it.Key, new(ArrayEqualityComparer<double>.Instance)))
            .ToFrozenDictionary();
        FrozenDictionary<ulong, (Edge<double>, double)[]> nodes = video.Nodes
            .Select(it => new KeyValuePair<ulong, (Edge<double>, double)[]>(it.Key, [.. it.Value
                .Select(it => (it, CalculateRandomProbability(it.RawCondition, video.Variables)))]))
            .ToFrozenDictionary();

        // BFS-like
        Queue<State> queue = [];
        queue.Enqueue(new(video.InitialNode, video.Variables.Values, 1, 1, new(-1, video.InitialNode)));
        while (queue.Count > 0)
        {
            State state = queue.Dequeue();

            HashSet<double[]> variableState = visited[state.Node];
            if (!variableState.Add(state.Variables))
                continue;

            PersistentState bestState = best[state.Node];
            int compareResult = bestState.CompareTo(state);
            if (compareResult > 0)
            {
                bestState.Depth = state.Depth;
                bestState.Probability = state.Probability;
                bestState.Path = state.LinkNode;
            }

            int newDepth = state.Depth + 1;
            if (newDepth > depthLimit)
                continue;
            int i = -1;
            foreach ((Edge<double> edge, double prob) in nodes[state.Node])
            {
                i++;
                double p = state.Probability * prob;
                if (p == 0 || !edge.CheckWithoutRandom(state.Variables))
                    continue;
                double[] copy = new double[state.Variables.Length];
                Array.Copy(state.Variables, copy, copy.Length);
                edge.PerformAction(copy);
                queue.Enqueue(new(edge.Next, copy, newDepth, p, new(i, edge.Next, state.LinkNode)));
            }
        }
        return best;
    }
}

public sealed class LinkNode(int index, ulong id, LinkNode? previous = null)
{
    public readonly int Index = index;
    public readonly ulong Id = id;
    public readonly LinkNode? Previous = previous;
}

public class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
    public static readonly ArrayEqualityComparer<T> Instance = new();

    public bool Equals(T[]? x, T[]? y) => x.AsSpan().SequenceEqual(y);
    public int GetHashCode([DisallowNull] T[] obj) => obj.Length switch
    {
        0 => 0,
        1 => HashCode.Combine(obj[0]),
        2 => HashCode.Combine(obj[0], obj[1]),
        3 => HashCode.Combine(obj[0], obj[1], obj[2]),
        4 => HashCode.Combine(obj[0], obj[1], obj[2], obj[3]),
        5 => HashCode.Combine(obj[0], obj[1], obj[2], obj[3], obj[4]),
        6 => HashCode.Combine(obj[0], obj[1], obj[2], obj[3], obj[4], obj[5]),
        7 => HashCode.Combine(obj[0], obj[1], obj[2], obj[3], obj[4], obj[5], obj[6]),
        _ => HashCode.Combine(obj[0], obj[1], obj[2], obj[3], obj[4], obj[5], obj[6], obj[7]),
    };
}

public struct State(ulong node, double[] variables, int depth, double probability, LinkNode linkNode)
{
    public ulong Node = node;
    public double[] Variables = variables;
    public int Depth = depth;
    public double Probability = probability;
    public LinkNode LinkNode = linkNode;
}

public sealed class PersistentState(int depth, double probability)
    : IComparable<PersistentState>, IComparable<State>, IEquatable<PersistentState>, IEquatable<State>
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
    public bool Equals(PersistentState? obj)
        => obj is not null && Depth == obj.Depth && Probability == obj.Probability;
    public bool Equals(State obj)
        => Depth == obj.Depth && Probability == obj.Probability;
    public override bool Equals(object? obj)
        => Equals(obj as PersistentState) || (obj is State other && Equals(other));
    public override int GetHashCode()
        => HashCode.Combine(Depth, Probability);
}