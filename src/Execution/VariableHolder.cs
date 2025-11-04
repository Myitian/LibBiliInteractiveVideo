using LibBiliInteractiveVideo.Execution.Compilation;
using System.Linq.Expressions;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution;

public class VariableHolder<T>
    where T : INumberBase<T>
{
    private readonly T[] _values;
    public static VariableHolder<T> Empty { get; } = new();

    private readonly Dictionary<string, (int, IndexExpression)> _variables;
    private readonly Dictionary<string, (int, IndexExpression)>.AlternateLookup<ReadOnlySpan<char>> _variablesLookup;
    public T[] Values { get; }
    public ExtraInfo[] ExtraInfo { get; } = [];
    public int Count => Values.Length;

    public VariableHolder(params IEnumerable<Variable<T>>? variables)
    {
        _variables = [];
        _variablesLookup = _variables.GetAlternateLookup<ReadOnlySpan<char>>();
        if (variables is null)
        {
            Values = _values = [];
            return;
        }
        List<(T, ExtraInfo)> vars = [];
        int i = 0;
        foreach (Variable<T> v in variables)
        {
            _variables[v.Id] = (i, Expression.ArrayAccess(ExpressionCache<T>.Array, ExpressionValueCache<int>.GetConstantOrNew(i)));
            vars.Add((v.Value, new()
            {
                ValueIndex = i,
                Id = v.Id,
                IsRandom = v.IsRandom,
                IsShow = v.IsShow,
                Name = v.Name
            }));
            i++;
        }
        Values = new T[vars.Count];
        ExtraInfo = new ExtraInfo[vars.Count];
        while (i-- > 0)
            (Values[i], ExtraInfo[i]) = vars[i];
        _values = [.. Values];
    }

    public void NextRandom()
    {
        foreach (ExtraInfo info in ExtraInfo)
            if (info.IsRandom)
                Values[info.ValueIndex] = T.CreateSaturating(Random.Shared.Next(1, 101));
    }

    public void Reset()
    {
        _values.CopyTo(Values, 0);
    }

    public T this[scoped ReadOnlySpan<char> name]
        => Values[GetVariableIndex(name)];
    public int GetVariableIndex(scoped ReadOnlySpan<char> name)
        => _variablesLookup[name].Item1;
    public IndexExpression GetArrayAccessExpression(scoped ReadOnlySpan<char> name)
        => _variablesLookup[name].Item2;
}
