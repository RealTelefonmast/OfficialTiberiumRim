using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TeleCore.Types.Utils;

namespace TeleCore.Types.Structs;

/// <summary>
///     A generic numeric wrapper struct that provides operator overloading and utility methods
///     for any unmanaged numeric type. Uses expression compilation for optimal performance.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Numeric<T> :
    IComparable,
    IComparable<T>,
    IComparable<Numeric<T>>,
    IConvertible,
    IEquatable<Numeric<T>>,
    IEquatable<T>
    where T : unmanaged
{
    private T _value;

    public T Value => _value;

    public static Numeric<T> Epsilon => new(NumericLib<T>.EpsilonGetter());
    public static Numeric<T> Zero => new(NumericLib<T>.ZeroGetter());
    public static Numeric<T> One => new(NumericLib<T>.OneGetter());
    public static Numeric<T> NegativeOne => new(NumericLib<T>.NegativeOneGetter());
    public static Numeric<T> NaN => new(NumericLib<T>.NaNGetter());

    public bool IsZero => NumericLib<T>.Equal(_value, NumericLib<T>.ZeroGetter());

    public bool IsNaN => _value switch
    {
        double d => double.IsNaN(d),
        float f => float.IsNaN(f),
        _ => false
    };

    public float AsPercent => _value switch
    {
        float f => f,
        double d => (float)d,
        decimal m => (float)m,
        _ => 0f
    };

    public unsafe int Bits => sizeof(T) * 8;

    public Numeric(T value)
    {
        _value = value;
    }

    #region Conversion Operators

    public static implicit operator T(Numeric<T> numeric)
    {
        return numeric._value;
    }

    public static implicit operator Numeric<T>(T value)
    {
        return new Numeric<T>(value);
    }

    #endregion

    #region Arithmetic Operators

    public static Numeric<T> operator +(Numeric<T> left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Addition(left._value, right._value));
    }

    public static Numeric<T> operator +(Numeric<T> left, T right)
    {
        return new Numeric<T>(NumericLib<T>.Addition(left._value, right));
    }

    public static Numeric<T> operator +(T left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Addition(left, right._value));
    }

    public static Numeric<T> operator -(Numeric<T> left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Subtraction(left._value, right._value));
    }

    public static Numeric<T> operator -(Numeric<T> left, T right)
    {
        return new Numeric<T>(NumericLib<T>.Subtraction(left._value, right));
    }

    public static Numeric<T> operator -(T left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Subtraction(left, right._value));
    }

    public static Numeric<T> operator -(Numeric<T> value)
    {
        return new Numeric<T>(NumericLib<T>.Subtraction(NumericLib<T>.ZeroGetter(), value._value));
    }

    public static Numeric<T> operator *(Numeric<T> left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Multiplication(left._value, right._value));
    }

    public static Numeric<T> operator *(Numeric<T> left, T right)
    {
        return new Numeric<T>(NumericLib<T>.Multiplication(left._value, right));
    }

    public static Numeric<T> operator *(T left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Multiplication(left, right._value));
    }

    public static Numeric<T> operator /(Numeric<T> left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Division(left._value, right._value));
    }

    public static Numeric<T> operator /(Numeric<T> left, T right)
    {
        return new Numeric<T>(NumericLib<T>.Division(left._value, right));
    }

    public static Numeric<T> operator /(T left, Numeric<T> right)
    {
        return new Numeric<T>(NumericLib<T>.Division(left, right._value));
    }

    public static Numeric<T> operator ++(Numeric<T> value)
    {
        return new Numeric<T>(NumericLib<T>.Addition(value._value, NumericLib<T>.OneGetter()));
    }

    public static Numeric<T> operator --(Numeric<T> value)
    {
        return new Numeric<T>(NumericLib<T>.Subtraction(value._value, NumericLib<T>.OneGetter()));
    }

    #endregion

    #region Bitwise Operators

    public static T operator <<(Numeric<T> value, int shift)
    {
        return NumericLib<T>.LeftShift(value._value, shift);
    }

    public static T operator >> (Numeric<T> value, int shift)
    {
        return NumericLib<T>.RightShift(value._value, shift);
    }

    public static T operator &(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseAnd(left._value, right._value);
    }

    public static T operator &(Numeric<T> left, T right)
    {
        return NumericLib<T>.BitwiseAnd(left._value, right);
    }

    public static T operator &(T left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseAnd(left, right._value);
    }

    public static T operator |(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseOr(left._value, right._value);
    }

    public static T operator |(Numeric<T> left, T right)
    {
        return NumericLib<T>.BitwiseOr(left._value, right);
    }

    public static T operator |(T left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseOr(left, right._value);
    }

    public static T operator ^(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseXor(left._value, right._value);
    }

    public static T operator ^(Numeric<T> left, T right)
    {
        return NumericLib<T>.BitwiseXor(left._value, right);
    }

    public static T operator ^(T left, Numeric<T> right)
    {
        return NumericLib<T>.BitwiseXor(left, right._value);
    }

    public static T operator ~(Numeric<T> value)
    {
        return NumericLib<T>.BitwiseNot(value._value);
    }

    #endregion

    #region Comparison Operators

    public static bool operator >(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.GreaterThan(left._value, right._value);
    }

    public static bool operator >(Numeric<T> left, T right)
    {
        return NumericLib<T>.GreaterThan(left._value, right);
    }

    public static bool operator >(T left, Numeric<T> right)
    {
        return NumericLib<T>.GreaterThan(left, right._value);
    }

    public static bool operator <(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.LessThan(left._value, right._value);
    }

    public static bool operator <(Numeric<T> left, T right)
    {
        return NumericLib<T>.LessThan(left._value, right);
    }

    public static bool operator <(T left, Numeric<T> right)
    {
        return NumericLib<T>.LessThan(left, right._value);
    }

    public static bool operator >=(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.GreaterThanOrEqual(left._value, right._value);
    }

    public static bool operator >=(Numeric<T> left, T right)
    {
        return NumericLib<T>.GreaterThanOrEqual(left._value, right);
    }

    public static bool operator >=(T left, Numeric<T> right)
    {
        return NumericLib<T>.GreaterThanOrEqual(left, right._value);
    }

    public static bool operator <=(Numeric<T> left, Numeric<T> right)
    {
        return NumericLib<T>.LessThanOrEqual(left._value, right._value);
    }

    public static bool operator <=(Numeric<T> left, T right)
    {
        return NumericLib<T>.LessThanOrEqual(left._value, right);
    }

    public static bool operator <=(T left, Numeric<T> right)
    {
        return NumericLib<T>.LessThanOrEqual(left, right._value);
    }

    public static bool operator ==(Numeric<T> left, Numeric<T> right)
    {
        return left._value.Equals(right._value);
    }

    public static bool operator ==(Numeric<T> left, T right)
    {
        return NumericLib<T>.Equal(left._value, right);
    }

    public static bool operator ==(T left, Numeric<T> right)
    {
        return NumericLib<T>.Equal(left, right._value);
    }

    public static bool operator !=(Numeric<T> left, Numeric<T> right)
    {
        return !left._value.Equals(right._value);
    }

    public static bool operator !=(Numeric<T> left, T right)
    {
        return NumericLib<T>.NotEqual(left._value, right);
    }

    public static bool operator !=(T left, Numeric<T> right)
    {
        return NumericLib<T>.NotEqual(left, right._value);
    }

    #endregion

    #region IComparable

    public int CompareTo(object? obj)
    {
        return obj switch
        {
            Numeric<T> other => NumericLib<T>.CompareTo(_value, other._value),
            T value => NumericLib<T>.CompareTo(_value, value),
            _ => throw new ArgumentException($"Object must be of type {typeof(T).Name} or Numeric<{typeof(T).Name}>")
        };
    }

    public int CompareTo(T other)
    {
        return NumericLib<T>.CompareTo(_value, other);
    }

    public int CompareTo(Numeric<T> other)
    {
        return NumericLib<T>.CompareTo(_value, other._value);
    }

    #endregion

    #region IEquatable

    public bool Equals(Numeric<T> other)
    {
        return _value.Equals(other._value);
    }

    public bool Equals(T other)
    {
        return _value.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Numeric<T> other => Equals(other),
            T value => Equals(value),
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(_value);
    }

    #endregion

    #region IConvertible

    public TypeCode GetTypeCode()
    {
        return Type.GetTypeCode(typeof(T));
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return !NumericLib<T>.Equal(_value, NumericLib<T>.ZeroGetter());
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return NumericConversionLib<T, byte>.Convert?.Invoke(_value) ?? default;
    }

    public char ToChar(IFormatProvider? provider)
    {
        return NumericConversionLib<T, char>.Convert?.Invoke(_value) ?? default;
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        throw new InvalidCastException($"Cannot convert {typeof(T).Name} to DateTime");
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return NumericConversionLib<T, decimal>.Convert?.Invoke(_value) ?? default;
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return NumericConversionLib<T, double>.Convert?.Invoke(_value) ?? default;
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return NumericConversionLib<T, short>.Convert?.Invoke(_value) ?? default;
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return NumericConversionLib<T, int>.Convert?.Invoke(_value) ?? default;
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return NumericConversionLib<T, long>.Convert?.Invoke(_value) ?? default;
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return NumericConversionLib<T, sbyte>.Convert?.Invoke(_value) ?? default;
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return NumericConversionLib<T, float>.Convert?.Invoke(_value) ?? default;
    }

    public string ToString(IFormatProvider? provider)
    {
        return _value.ToString() ?? string.Empty;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return Convert.ChangeType(_value, conversionType, provider);
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return NumericConversionLib<T, ushort>.Convert?.Invoke(_value) ?? default;
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return NumericConversionLib<T, uint>.Convert?.Invoke(_value) ?? default;
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return NumericConversionLib<T, ulong>.Convert?.Invoke(_value) ?? default;
    }

    #endregion

    public override string ToString()
    {
        return _value.ToString() ?? string.Empty;
    }
}