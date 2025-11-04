using LibBiliInteractiveVideo.API;
using LibBiliInteractiveVideo.Execution.Compilation;
using System.Linq.Expressions;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution;

public static class Edge
{
    public static Edge<double> ConvertFromAPI(XSteinEdgeinfoV2.Choice choice, VariableHolder<double> variables, CompileCache<double>? cache = null)
    {
        string? cond = choice.Condition;
        string? action = choice.NativeAction;
        Func<double[], bool>? condition = cache?.GetConditionOrCompute(cond, CompileConditionLambda)
            ?? CompileConditionLambda(cond);
        Func<double[], bool>? conditionWithoutRandom = cache?.GetConditionWithoutRandomOrCompute(cond, CompileConditionWithoutRandomLambda)
            ?? CompileConditionWithoutRandomLambda(cond);
        Action<double[]>? nativeAction = cache?.GetNativeActionOrCompute(action, CompileNativeActionLambda)
            ?? CompileNativeActionLambda(action);
        return new(choice.Id, choice.Option, condition, conditionWithoutRandom, nativeAction, cond, action);

        Func<double[], bool>? CompileConditionLambda(scoped ReadOnlySpan<char> condition)
        {
            Expression expr = Edge<double>.CompileCondition(condition, variables);
            if (expr == ExpressionCache.True)
                return null;
            if (expr == ExpressionCache.False)
                return ExpressionCache<double>.AlwaysFalse;
            return Expression.Lambda<Func<double[], bool>>(expr, ExpressionCache<double>.Array).Compile();
        }
        Func<double[], bool>? CompileConditionWithoutRandomLambda(scoped ReadOnlySpan<char> condition)
        {
            Expression expr = Edge<double>.CompileConditionWithoutRandom(condition, variables);
            if (expr == ExpressionCache.True)
                return null;
            if (expr == ExpressionCache.False)
                return ExpressionCache<double>.AlwaysFalse;
            return Expression.Lambda<Func<double[], bool>>(expr, ExpressionCache<double>.Array).Compile();
        }
        Action<double[]>? CompileNativeActionLambda(scoped ReadOnlySpan<char> nativeAction)
        {
            Expression expr = Edge<double>.CompileNativeAction(nativeAction, variables);
            if (expr == ExpressionCache.NoOp)
                return null;
            return Expression.Lambda<Action<double[]>>(expr, ExpressionCache<double>.Array).Compile();
        }
    }
}
public class Edge<T>(
    ulong next,
    string? option = null,
    Func<T[], bool>? condition = null,
    Func<T[], bool>? conditionWithoutRandom = null,
    Action<T[]>? nativeAction = null,
    string? rawCondition = null,
    string? rawNativeAction = null)
    where T : INumberBase<T>, IComparable<T>, IComparisonOperators<T, T, bool>
{
    private readonly Func<T[], bool>? _condition = condition;
    private readonly Func<T[], bool>? _conditionWithoutRandom = conditionWithoutRandom;
    private readonly Action<T[]>? _nativeAction = nativeAction;
    public ulong Next { get; } = next;
    public string? Option { get; } = option;
    public string? RawCondition { get; } = rawCondition;
    public string? RawNativeAction { get; } = rawNativeAction;
    public bool Check(T[] variables) => _condition?.Invoke(variables) ?? true;
    public bool CheckWithoutRandom(T[] variables) => _conditionWithoutRandom?.Invoke(variables) ?? true;
    public void PerformAction(T[] variables) => _nativeAction?.Invoke(variables);

    public static Expression CompileNativeAction(scoped ReadOnlySpan<char> nativeAction, VariableHolder<T> variables)
    {
        return CompileNativeAction<NamedNativeActionEnumerator<T>, NamedNativeActionEnumerator<T>>(new(nativeAction), variables);
    }
    public static Expression CompileNativeAction<TEnumerable, TEnumerator>(scoped TEnumerable nativeAction, VariableHolder<T> variables)
        where TEnumerable : IGetEnumerator<TEnumerator, NamedNativeAction<T>>, allows ref struct
        where TEnumerator : IEnumerator<NamedNativeAction<T>>, allows ref struct
    {
        List<Expression> body = [];
        foreach (NamedNativeAction<T> action in nativeAction)
        {
            if (action.NativeAction.Op != NativeActionOperation.Assign && T.IsZero(action.NativeAction.Value))
                continue;
            body.Add(action.NativeAction.CreateExpression(
                variables.GetArrayAccessExpression(action.Store),
                variables.GetArrayAccessExpression(action.Load)));
        }
        return body.Count == 0 ? ExpressionCache.NoOp : Expression.Block(body);
    }

    public static Expression CompileCondition(scoped ReadOnlySpan<char> condition, VariableHolder<T> variables)
    {
        return CompileCondition<NamedConditionEnumerator<T>, NamedConditionEnumerator<T>>(new(condition), variables);
    }
    public static Expression CompileCondition<TEnumerable, TEnumerator>(scoped TEnumerable condition, VariableHolder<T> variables)
        where TEnumerable : IGetEnumerator<TEnumerator, NamedCondition<T>>, allows ref struct
        where TEnumerator : IEnumerator<NamedCondition<T>>, allows ref struct
    {
        Expression? expr = null;
        foreach (NamedCondition<T> cond in condition)
        {
            Expression current = cond.Condition.CreateExpression(variables.GetArrayAccessExpression(cond.Name));
            expr = expr is null ? current : Expression.AndAlso(expr, current);
        }
        return expr ?? ExpressionCache.True;
    }

    public static Expression CompileConditionWithoutRandom(scoped ReadOnlySpan<char> condition, VariableHolder<T> variables)
    {
        return CompileConditionWithoutRandom<NamedConditionEnumerator<T>, NamedConditionEnumerator<T>>(new(condition), variables);
    }
    public static Expression CompileConditionWithoutRandom<TEnumerable, TEnumerator>(scoped TEnumerable condition, VariableHolder<T> variables)
        where TEnumerable : IGetEnumerator<TEnumerator, NamedCondition<T>>, allows ref struct
        where TEnumerator : IEnumerator<NamedCondition<T>>, allows ref struct
    {
        Expression? expr = null;
        foreach (NamedCondition<T> cond in condition)
        {
            if (variables.ExtraInfo[variables.GetVariableIndex(cond.Name)].IsRandom)
                continue;
            Expression current = cond.Condition.CreateExpression(variables.GetArrayAccessExpression(cond.Name));
            expr = expr is null ? current : Expression.AndAlso(expr, current);
        }
        return expr ?? ExpressionCache.True;
    }
}
