using System.Collections.Generic;
using System.Diagnostics;

namespace TeleCore.Types.Structs;

/// <summary>
///     A key to define a two-way relationship between two objects.
///     Meant for tuples of two object of the same type, to ignore ordering of the tuple.
/// </summary>
[DebuggerDisplay("{GetHashCode()}")]
public readonly record struct TwoWayKey<T>
{
    public TwoWayKey(T a, T b)
    {
        var hashA = a?.GetHashCode() ?? 0;
        var hashB = b?.GetHashCode() ?? 0;
        var compare = hashA.CompareTo(hashB);
        if (compare > 0)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public T A { get; }
    public T B { get; }

    public bool IsValid => A != null && B != null;

    public bool Equals(TwoWayKey<T> other)
    {
        if (!Equals(A, other.A)) return false;
        if (!Equals(B, other.B)) return false;
        return GetHashCode().Equals(other.GetHashCode());
    }

    public static implicit operator TwoWayKey<T>((T, T) tuple)
    {
        return new TwoWayKey<T>(tuple.Item1, tuple.Item2);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (EqualityComparer<T>.Default.GetHashCode(A) * 397) ^ EqualityComparer<T>.Default.GetHashCode(B);
        }
    }
}