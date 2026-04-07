using System;
using System.Collections.Generic;

namespace TeleCore.Math;

/// <summary>
/// Generic numeric operations library using expression compilation for performance.
/// Supports any unmanaged numeric type including integers, floats, and enums.
/// </summary>
public static partial class NumericLib<T> where T : unmanaged
{
    // Arithmetic
    public static readonly Func<T, T, T> Addition;
    public static readonly Func<T, T, T> Subtraction;
    public static readonly Func<T, T, T> Multiplication;
    public static readonly Func<T, T, T> Division;

    // Comparison
    public static readonly Func<T, T, int> CompareTo;
    public static readonly Func<T, T, bool> GreaterThan;
    public static readonly Func<T, T, bool> LessThan;
    public static readonly Func<T, T, bool> Equal;
    public static readonly Func<T, T, bool> NotEqual;
    public static readonly Func<T, T, bool> GreaterThanOrEqual;
    public static readonly Func<T, T, bool> LessThanOrEqual;

    // Constants
    public static readonly Func<T> EpsilonGetter;
    public static readonly Func<T> ZeroGetter;
    public static readonly Func<T> OneGetter;
    public static readonly Func<T> NegativeOneGetter;
    public static readonly Func<T> NaNGetter;

    // Math
    public static readonly Func<IEnumerable<T>, T> Sum;
    public static readonly Func<T, T, T, T> Clamp;
    public static readonly Func<T, int, T> Round;
    public static readonly Func<T, T, T> Min;
    public static readonly Func<T, T, T> Max;
    public static readonly Func<T, T> Abs;

    // Bitwise
    public static readonly Func<T, int, T> LeftShift;
    public static readonly Func<T, int, T> RightShift;
    public static readonly Func<T, T, T> BitwiseAnd;
    public static readonly Func<T, T, T> BitwiseOr;
    public static readonly Func<T, T, T> BitwiseXor;
    public static readonly Func<T, T> BitwiseNot;

    public static Numeric<T> Zero => new(ZeroGetter());
    public static Numeric<T> One => new(OneGetter());

    static NumericLib()
    {
        // Arithmetic
        _ = TryCreate(out Addition, CreateAddFunc);
        _ = TryCreate(out Subtraction, CreateSubtractFunc);
        _ = TryCreate(out Multiplication, CreateMultiplicationFunc);
        _ = TryCreate(out Division, CreateDivisionFunc);

        // Comparison
        _ = TryCreate(out CompareTo, CreateCompareTo);
        _ = TryCreate(out GreaterThan, CreateGreaterThan);
        _ = TryCreate(out LessThan, CreateLessThan);
        _ = TryCreate(out Equal, CreateEqual);
        _ = TryCreate(out NotEqual, CreateNotEqual);
        _ = TryCreate(out GreaterThanOrEqual, CreateGreaterThanOrEqual);
        _ = TryCreate(out LessThanOrEqual, CreateLessThanOrEqual);

        // Constants
        _ = TryCreate(out EpsilonGetter, CreateEpsilonGetter);
        _ = TryCreate(out ZeroGetter, CreateZeroGetter);
        _ = TryCreate(out OneGetter, CreateOneGetter);
        _ = TryCreate(out NegativeOneGetter, CreateNegativeOneGetter);
        _ = TryCreate(out NaNGetter, CreateNaNGetter);

        // Math
        _ = TryCreate(out Sum, CreateSumFunc);
        _ = TryCreate(out Min, CreateMinFunc);
        _ = TryCreate(out Max, CreateMaxFunc);
        _ = TryCreate(out Clamp, CreateClampFunc);
        _ = TryCreate(out Round, CreateRoundFunc);
        _ = TryCreate(out Abs, CreateAbsFunc);

        // Bitwise
        _ = TryCreate(out LeftShift, CreateLeftShiftFunc);
        _ = TryCreate(out RightShift, CreateRightShiftFunc);
        _ = TryCreate(out BitwiseAnd, CreateBitwiseAndFunc);
        _ = TryCreate(out BitwiseOr, CreateBitwiseOrFunc);
        _ = TryCreate(out BitwiseXor, CreateBitwiseXorFunc);
        _ = TryCreate(out BitwiseNot, CreateBitwiseNotFunc);
    }

    private static bool TryCreate<TFunc>(out TFunc func, Func<TFunc> createFunc)
    {
        func = default!;
        try
        {
            func = createFunc();
        }
        catch
        {
            // Operation not supported for this type
        }
        return func != null;
    }
}
