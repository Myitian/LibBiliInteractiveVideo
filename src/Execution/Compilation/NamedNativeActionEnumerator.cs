using System.Buffers;
using System.Collections;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public interface IGetEnumerator<T, TItem>
    where T : IEnumerator<TItem>, allows ref struct
    where TItem : allows ref struct
{
    T GetEnumerator();
}
public ref struct NamedNativeActionEnumerator<T>(ReadOnlySpan<char> expression)
    : IEnumerator<NamedNativeAction<T>>, IGetEnumerator<NamedNativeActionEnumerator<T>, NamedNativeAction<T>>
    where T : INumberBase<T>
{
    private static readonly SearchValues<char> OpsWithSemicolon = SearchValues.Create("+-;");

    private ReadOnlySpan<char> _expression = expression;
    private NamedNativeAction<T> _current = default;
    public readonly NamedNativeAction<T> Current => _current;
    public bool MoveNext()
    {
        ReadOnlySpan<char> expr = _expression;
        if (expr.IsEmpty)
            return false;
        expr = expr.TrimStart();
        int eqIndex = expr.IndexOf('=');
        if (eqIndex < 0)
            goto FAILED;
        _current.Store = expr[..eqIndex].TrimEnd();
        expr = expr[eqIndex..];
        int opIndex = expr.IndexOfAny(OpsWithSemicolon);
        if (opIndex >= 0)
        {
            switch (expr[opIndex])
            {
                case '+':
                    _current.Load = expr[1..opIndex].Trim();
                    _current.NativeAction.Op = NativeActionOperation.Add;
                    expr = expr[opIndex..];
                    opIndex = expr.IndexOf(';');
                    break;
                case '-':
                    _current.Load = expr[1..opIndex].Trim();
                    _current.NativeAction.Op = NativeActionOperation.Subtract;
                    expr = expr[opIndex..];
                    opIndex = expr.IndexOf(';');
                    break;
                default:
                    _current.Load = _current.Store;
                    _current.NativeAction.Op = NativeActionOperation.Assign;
                    break;
            }
        }
        else
        {
            _current.Load = _current.Store;
            _current.NativeAction.Op = NativeActionOperation.Assign;
        }
        if (opIndex >= 0)
        {
            if (!T.TryParse(expr[1..opIndex].Trim(), null, out T? value))
                goto FAILED;
            _current.NativeAction.Value = value;
            _expression = expr[(opIndex + 1)..];
        }
        else
        {
            if (!T.TryParse(expr[1..].Trim(), null, out T? value))
                goto FAILED;
            _current.NativeAction.Value = value;
            _expression = [];
        }
        return true;
    FAILED:
        _expression = default;
        return false;
    }
    public readonly NamedNativeActionEnumerator<T> GetEnumerator() => this;
    public readonly void Dispose() { }
    public readonly void Reset() => throw new NotSupportedException();
    readonly object IEnumerator.Current => throw new NotSupportedException();
}
