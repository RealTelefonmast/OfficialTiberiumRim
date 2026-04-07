using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TeleCore.Defs.Values;
using TeleCore.Logging;
using TeleCore.Math;
using TeleCore.Primitive.Immutable;
using TeleCore.Static;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Primitive;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct UnsafeDefValue<TDef, TValue>
    where TDef : Def
    where TValue : unmanaged
{
    [FieldOffset(0)] 
    public int defID; //4 bytes
    [FieldOffset(4)] 
    public Numeric<TValue> _value; //Up to 8 bytes

    public UnsafeDefValue(Def def, Numeric<TValue> value)
    {
        defID = DefIDStack.ToID(def);
        _value = value;
    }

    public TDef Def => DefIDStack.ToDef<TDef>(defID);
}

public unsafe struct UnsafeValueStack<TDef, TValue>
    where TDef : Def
    where TValue : unmanaged
{
    private const int maxLength = 128;
    private const int byteLength = maxLength * (4 + 8);

    private fixed byte _stack[byteLength];
    private ushort curInd;
    private Numeric<TValue> _totalValue;

    //States
    public int Length => curInd;
    public Numeric<TValue> TotalValue => _totalValue;
    public bool IsValid => false;
    public bool Invalid =>  false;
    
    public bool IsEmpty => _totalValue.IsZero;

    public UnsafeDefValue<TDef, TValue> this[int ind]
    {
        get
        {
            if (ind < 0 || ind >= curInd)
                throw new IndexOutOfRangeException();
            
            fixed (byte* ptr = _stack)
            {
                var defValue = (UnsafeDefValue<TDef, TValue>*) ptr;
                return defValue[ind];
            }
        }
    }
    
    //Stack Info
    public IEnumerable<TDef> Defs
    {
        get
        {
            for(var i=0; i<curInd; i++)
                yield return this[i].Def;
        }
    }

    public UnsafeValueStack()
    {
        
    }
    
    public UnsafeDefValue<TDef, TValue> TryGetWithFallback(TDef key, UnsafeDefValue<TDef, TValue> fallback)
    {
        return TryGetValue(key, out _, out var value) ? value : fallback;
    }

    public bool TryGetValue(TDef key, out int index, out UnsafeDefValue<TDef, TValue> value)
    {
        index = -1;
        var tmp = value = new UnsafeDefValue<TDef, TValue>(key, Numeric<TValue>.Zero);
        if(curInd == 0) return false;
        for (var i = 0; i < curInd; i++)
        {
            tmp = this[i];
            if (tmp.Def != key) continue;
            value = tmp;
            index = i;
            return true;
        }

        return false;
    }
    
    private unsafe void TryAddOrSet(UnsafeDefValue<TDef, TValue> newValue)
    {
        if (!TryGetValue(newValue.Def, out var index, out var previous))
        {
            //Add onto stack
            var ptr = (byte*)&newValue;
            var nxt = curInd++;
            for (int i = 0; i < 12; i++)
            {
                _stack[nxt + i] = ptr[i];
            }

            index = nxt;
        }
        else
        {
            //Set new value
            var ptr = (byte*)&newValue;
            for (int i = 0; i < 12; i++)
            {
                _stack[index + i] = ptr[i];
            }
        }

        if (index < 0) return; //Failed to add

        //Get Delta
        //TODO:var delta = this[index] - previous;
       //TODO: _totalValue += delta.Value;
    }
    
}

/// <summary>
///     Manages any Def as a numeric value in a stack.
/// </summary>
/// <typeparam name="TDef">The <see cref="Def" /> of the stack.</typeparam>
/// <typeparam name="TValue">The numeric type of the stack.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public struct DefValueStack<TDef, TValue> : IExposable
    where TDef : Def
    where TValue : unmanaged
{
    private LightImmutableArray<DefValue<TDef, TValue>> _stack;
    private Numeric<TValue> _totalValue;
    
    //States
    public int Length => _stack.Length;
    public Numeric<TValue> TotalValue => _totalValue;
    public bool IsValid => !_stack.IsNullOrEmpty;
    public bool Invalid =>_stack.IsNullOrEmpty;
    
    public bool IsEmpty => _totalValue.IsZero;
    
    public static implicit operator DefValueStack<TDef, TValue>(DefValue<TDef, TValue> value) =>new DefValueStack<TDef, TValue>(value);
    
    //Stack Info
    public IEnumerable<TDef> Defs => _stack.Select(value => value.Def);
    public ICollection<DefValue<TDef, TValue>> Values => _stack;

    public DefValue<TDef, TValue> this[int index] => _stack[index];

    public DefValue<TDef, TValue> this[TDef def]
    {
        get => TryGetWithFallback(def, new DefValue<TDef, TValue>(def, Numeric<TValue>.Zero));
        set => TryAddOrSet(value);
    }

    public static DefValueStack<TDef, TValue> Empty => new();

    public void ExposeData()
    {
        var listTemp = new List<DefValueLoadable<TDef, TValue>>();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            foreach (var val in _stack)
            {
                listTemp.Add(val);
            }
        }
        
        Scribe_Values.Look(ref _totalValue, "totalValue");
        Scribe_Collections.Look(ref listTemp, "stack", LookMode.Deep);
        //Scribe_Arrays.Look(ref _stack, "stack");
        
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            _stack = new LightImmutableArray<DefValue<TDef, TValue>>();
            foreach (var val in listTemp)
            {
                _stack = _stack.Add(val);
            }
        }
    }
    
    public DefValueStack()
    {
        _stack = LightImmutableArray<DefValue<TDef, TValue>>.Empty;
        _totalValue = Numeric<TValue>.Zero;
        
    }

    public DefValueStack(DefValueStack<TDef, TValue> other) : this()
    {
        if (!other.IsValid)
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new FlowValueStack from invalid FlowValueStack.");
            return;
        }

        _stack = other._stack;
        _totalValue = other._totalValue;
    }

    public DefValueStack(DefValue<TDef,TValue> value) : this()
    {
        if (value.Def == null)
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new FlowValueStack from null DefValue.");
            return;
        }

        _stack = new LightImmutableArray<DefValue<TDef, TValue>>(value); //new ImmutableArray<DefValue<TDef, TValue>>().Add(value);
        _totalValue = value.Value;
    }
    
    /*public DefValueStack(IDictionary<TDef, TValue> source, TValue maxCapacity) : this(maxCapacity)
    {
        if (source.EnumerableNullOrEmpty())
        {
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef,TValue>[source.Count];
        var i = 0;
        foreach (var value in source)
        {
            _stack[i] = new DefValue<TDef>(value.Key, value.Value);
            AdjustTotal(value.Value);
            i++;
        }
    }

    public DefValueStack(ICollection<TDef> defs, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (!defs.Any())
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from Empty {typeof(TDef)} Array.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[defs.Count];
        var i = 0;
        foreach (var def in defs)
        {
            _stack[i] = new DefValue<TDef>(def, 0);
            i++;
        }
    }

    public DefValueStack(DefValue<TDef>[] source, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (source.NullOrEmpty())
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from NullOrEmpty DefFloat Array.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[source.Length];
        var i = 0;
        foreach (var value in source)
        {
            _stack[i] = value;
            AdjustTotal(value.Value);
            i++;
        }
    }

    public DefValueStack(DefValueStack<TDef, TValue> other, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (!other.IsValid)
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from invalid DefValueStack.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[other._stack.Length];
        for (var i = 0; i < _stack.Length; i++)
        {
            _stack[i] = other._stack[i];
        }
        _totalValue = other._totalValue;
    }*/
    
    private int IndexOf(TDef def)
    {
        for (var i = 0; i < _stack.Length; i++)
            if (_stack[i].Def == def)
                return i;

        return -1;
    }

    public DefValue<TDef, TValue> TryGetWithFallback(TDef key, DefValue<TDef, TValue> fallback)
    {
        return TryGetValue(key, out _, out var value) ? value : fallback;
    }

    public bool TryGetValue(TDef key, out int index, out DefValue<TDef, TValue> value)
    {
        index = -1;
        var tmp = value = new DefValue<TDef, TValue>(key, Numeric<TValue>.Zero);
        if(_stack.IsNullOrEmpty) return false;
        for (var i = 0; i < _stack.Length; i++)
        {
            tmp = _stack[i];
            if (tmp.Def != key) continue;
            value = tmp;
            index = i;
            return true;
        }

        return false;
    }

    private void TryAddOrSet(DefValue<TDef, TValue> newValue)
    {
        if (newValue.Value.IsNaN)
        {
            TLog.Warning("Caught NaN!");
            newValue = new DefValue<TDef, TValue>(newValue.Def, Numeric<TValue>.Zero);
        }
        
        if (!TryGetValue(newValue.Def, out var index, out var previous))
        {
            //Add onto stack
            if (_stack.IsNullOrEmpty)
            {
                _stack = new LightImmutableArray<DefValue<TDef, TValue>>(newValue);
            }
            else
            {
                _stack = _stack.Add(newValue);
            }

            index = _stack.Length - 1;
        }
        else
        {
            //Set new value
            _stack = _stack.Replace(previous, newValue);
        }

        if (index < 0) return; //Failed to add

        //Get Delta
        var delta = _stack[index] - previous;
        _totalValue += delta.Value;
    }

    public override string ToString()
    {
        if (_stack == null || Length == 0) return "Empty Stack";
        var sb = new StringBuilder();
        sb.AppendLine($"[TOTAL: {TotalValue}]");
        for (var i = 0; i < Length; i++)
        {
            var value = _stack[i];
            sb.AppendLine($"[{i}] {value}");
        }

        return sb.ToString();
    }

    public IEnumerator<DefValue<TDef, TValue>> GetEnumerator()
    {
        return _stack.Cast<DefValue<TDef, TValue>>()
            .GetEnumerator(); //(IEnumerator<DefFloat<TDef>>)_stack.GetEnumerator();
    }

    public void Reset()
    {
        _totalValue = Numeric<TValue>.Zero;
        _stack = _stack.Clear();
    }
    
    #region Math
    
    #region Value Math

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> a, TValue b)
    {
        if (a._stack.IsNullOrEmpty) return a;
        foreach (var value in a.Values) 
            a[value.Def] *= b;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> a, TValue b)
    {
        if (a._stack.IsNullOrEmpty) return a;
        if (new Numeric<TValue>(b).IsZero)
        {
            return new DefValueStack<TDef, TValue>();
        }
        foreach (var value in a.Values) 
            a[value.Def] /= b;
        return a;
    }

    #endregion

    #region Stack Math

    public static DefValueStack<TDef, TValue> operator +(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        if (b._stack.IsNullOrEmpty) return a;
        foreach (var value in b.Values) 
            a[value.Def] += b[value.Def];
        return a;
    }

    public static DefValueStack<TDef, TValue> operator -(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        if (b._stack.IsNullOrEmpty) return a;
        foreach (var value in b.Values) 
            a[value.Def] -= b[value.Def];
        return a;
    }

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        if (a._stack.IsNullOrEmpty) return a;
        foreach (var value in a.Values) 
            a[value.Def] *= b[value.Def];
        return a;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        if (a._stack.IsNullOrEmpty) return a;
        foreach (var value in a.Values)
        {
            if (b[value.Def].Value.IsZero)
            {
                a[value.Def] = new DefValue<TDef, TValue>(value.Def, Numeric<TValue>.Zero);
                continue;
            }
            a[value.Def] /= b[value.Def];
        }

        return a;
    }

    #endregion

    #region DefValue Math

    public static DefValueStack<TDef, TValue> operator +(DefValueStack<TDef, TValue> a, DefValue<TDef, TValue> value)
    {
        a[value.Def] += value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator -(DefValueStack<TDef, TValue> a, DefValue<TDef, TValue> value)
    {
        a[value.Def] -= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> a, DefValue<TDef, TValue> value)
    {
        if (a._stack.IsNullOrEmpty) return a;
        a[value.Def] *= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> a, DefValue<TDef, TValue> value)
    {
        if (a._stack.IsNullOrEmpty) return a;
        if (value.Value.IsZero)
        {
            return new DefValueStack<TDef, TValue>();
        }
        a[value.Def] /= value;
        return a;
    }

    #endregion

    #endregion

    #region Comparision

    #region Stack

    public static bool operator >(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue > b.TotalValue;
    }

    public static bool operator <(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue < b.TotalValue;
    }
    
    public static bool operator ==(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue == b.TotalValue;
    }


    public static bool operator !=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue != b.TotalValue;
    }


    public static bool operator >=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue >= b.TotalValue;
    }


    public static bool operator <=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue <= b.TotalValue;
    }

    #endregion

    #region Value

    public static bool operator >(TValue a, DefValueStack<TDef, TValue> b)
    {
        return a > b._totalValue;
    }
    
    
    public static bool operator<(TValue a, DefValueStack<TDef, TValue> b)
    {
        return a < b._totalValue;
    }
    
    public static bool operator >(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue > b;
    }

    public static bool operator <(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue < b;
    }
    
    public static bool operator ==(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue == b;
    }


    public static bool operator !=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue != b;
    }


    public static bool operator >=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue >= b;
    }


    public static bool operator <=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue <= b;
    }

    #endregion

    public bool Equals(DefValueStack<TDef, TValue> other)
    {
        return _stack.Equals(other._stack)
               && _totalValue == other._totalValue;
    }

    public override bool Equals(object? obj)
    {
        return obj is DefValueStack<TDef, TValue> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode =  _stack.GetHashCode();
            hashCode = (hashCode * 397) ^ _totalValue.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}