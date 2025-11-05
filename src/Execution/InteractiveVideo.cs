using LibBiliInteractiveVideo.API;
using LibBiliInteractiveVideo.Execution.Compilation;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution;

public struct Node()
{
    public Edge<double>[] Edges = [];
    public ulong Id;
    public ulong Cid;

    public Node(ulong id, ulong cid, params Edge<double>[] edges) : this()
    {
        Edges = edges;
        Id = id;
        Cid = cid;
    }
}

public static class InteractiveVideo
{
    public static InteractiveVideo<double> ConvertFromAPI(params IEnumerable<XSteinEdgeinfoV2.Data> nodes)
    {
        CompileCache<double> cache = new();
        Dictionary<ulong, Node> nodeDict = [];
        VariableHolder<double>? variables = null;
        ulong initialNode = 0;
        foreach (XSteinEdgeinfoV2.Data node in nodes)
        {
            if (variables is null)
            {
                variables = new(node.HiddenVars?.Select(Variable.ConvertFromAPI));
                initialNode = node.EdgeId;
            }
            if (node.Edges.Questions is null)
            {
                nodeDict[node.EdgeId] = new(node.EdgeId, 0, []);
                continue;
            }
            List<Edge<double>> tmp = [];
            foreach (XSteinEdgeinfoV2.Question q in node.Edges.Questions)
            {
                if (q.Choices is null)
                    continue;
                foreach (XSteinEdgeinfoV2.Choice c in q.Choices)
                {
                    tmp.Add(Edge.ConvertFromAPI(c, variables, cache));
                }
            }
            nodeDict[node.EdgeId] = new(node.EdgeId, 0, [.. tmp]);
        }
        return new(nodeDict, variables ?? VariableHolder<double>.Empty, initialNode);
    }
    public static InteractiveVideo<double> ConvertFromAPI(params IEnumerable<(XSteinEdgeinfoV2.Data, ulong)> nodes)
    {
        CompileCache<double> cache = new();
        Dictionary<ulong, Node> nodeDict = [];
        VariableHolder<double>? variables = null;
        ulong initialNode = 0;
        foreach ((XSteinEdgeinfoV2.Data node, ulong cid) in nodes)
        {
            if (variables is null)
            {
                variables = new(node.HiddenVars?.Select(Variable.ConvertFromAPI));
                initialNode = node.EdgeId;
            }
            if (node.Edges.Questions is null)
            {
                nodeDict[node.EdgeId] = new(node.EdgeId, cid, []);
                continue;
            }
            List<Edge<double>> tmp = [];
            foreach (XSteinEdgeinfoV2.Question q in node.Edges.Questions)
            {
                if (q.Choices is null)
                    continue;
                foreach (XSteinEdgeinfoV2.Choice c in q.Choices)
                {
                    tmp.Add(Edge.ConvertFromAPI(c, variables, cache));
                }
            }
            nodeDict[node.EdgeId] = new(node.EdgeId, cid, [.. tmp]);
        }
        return new(nodeDict, variables ?? VariableHolder<double>.Empty, initialNode);
    }
}
public class InteractiveVideo<T>(Dictionary<ulong, Node> nodes, VariableHolder<T> variables, ulong initialNode = 1)
    where T : INumberBase<T>, IComparable<T>, IComparisonOperators<T, T, bool>
{
    public Dictionary<ulong, Node> Nodes { get; } = nodes;
    public VariableHolder<T> Variables { get; } = variables;
    public ulong InitialNode { get; } = initialNode;
}
