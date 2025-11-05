using System.Diagnostics.CodeAnalysis;

namespace ResolveShortestPath;

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
