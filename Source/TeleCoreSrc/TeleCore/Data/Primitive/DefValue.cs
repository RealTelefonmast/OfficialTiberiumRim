using System.Runtime.InteropServices;
using TeleCore.GameData.Defs.Values;
using TeleCore.Logging;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Primitive;

/// <summary>
///     Wraps any <see cref="Def" /> Type into a struct, attaching a numeric value
/// </summary>
/// <typeparam name="TDef">The <see cref="Def" /> Type of the value.</typeparam>
/// <typeparam name="TValue">The numeric Type of the value.</typeparam>
public struct DefValue<TDef, TValue> 
    where TDef : Def
    where TValue : unmanaged
{
    public TDef Def { get; }
    public Numeric<TValue> Value { get; set; }
    
    public static implicit operator DefValue<TDef, TValue>((TDef Def, Numeric<TValue> Value) value) => new(value.Def, value.Value);
    public static implicit operator TDef(DefValue<TDef, TValue> def) => def.Def;
    public static explicit operator Numeric<TValue>(DefValue<TDef, TValue> def) => def.Value;


    public static DefValue<TDef, TValue> Invalid => new(null, Numeric<TValue>.Zero);

    public DefValue(DefValueLoadable<TDef, TValue> defValue)
    {
        Def = defValue.Def;
        Value = defValue.Value;
    }

    public DefValue(TDef def, TValue value)
    {
        Def = def;
        Value = value;
    }
    
    #region Math

    public static DefValue<TDef, TValue> operator +(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value + b);
    }

    public static DefValue<TDef, TValue> operator -(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value - b);
    }

    public static DefValue<TDef, TValue> operator *(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value * b);
    }

    public static DefValue<TDef, TValue> operator /(DefValue<TDef, TValue> a, TValue b)
    {
        if (new Numeric<TValue>(b).IsZero) return new DefValue<TDef, TValue>(a.Def, Numeric<TValue>.Zero);
        return new DefValue<TDef, TValue>(a.Def, a.Value / b);
    }

    public static DefValue<TDef, TValue> operator +(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def)
        {
            TLog.Warning($"Tried to add two DefValues with different Defs. {a} + {b}");
            return Invalid;
        }
        return new DefValue<TDef, TValue>(a.Def, a.Value + b.Value);
    }

    public static DefValue<TDef, TValue> operator -(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def)
        {
            TLog.Warning($"Tried to subtract two DefValues with different Defs. {a} - {b}");
            return Invalid;
        }
        return new DefValue<TDef, TValue>(a.Def, a.Value - b.Value);
    }

    public static DefValue<TDef, TValue> operator *(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def)
        {
            TLog.Warning($"Tried to multiply two DefValues with different Defs. {a} * {b}");
            return Invalid;
        }
        return new DefValue<TDef, TValue>(a.Def, a.Value * b.Value);
    }

    public static DefValue<TDef, TValue> operator /(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def)
        {
            TLog.Warning($"Tried to divide two DefValues with different Defs. {a} / {b}");
            return Invalid;
        }
        if (b.Value.IsZero) return new DefValue<TDef, TValue>(a.Def, Numeric<TValue>.Zero);
        return new DefValue<TDef, TValue>(a.Def, a.Value / b.Value);
    }

    #endregion

    #region Comparision

    public static bool operator >(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value > b;
    }

    public static bool operator <(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value < b;
    }


    public static bool operator ==(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value == b;
    }


    public static bool operator !=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value == b;
    }


    public static bool operator >=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value >= b;
    }


    public static bool operator <=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value <= b;
    }

    #endregion

    public override string ToString()
    {
        return $"({Def}, {Value})";
    }
}