using System;
using System.Collections.Generic;
using TeleCore.Lib;
using TeleCore.Math;
using Verse;

namespace TeleCore.Shared.Unsafe;

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
    public bool Invalid => false;

    public bool IsEmpty => _totalValue.IsZero;

    public UnsafeDefValue<TDef, TValue> this[int ind]
    {
        get
        {
            if (ind < 0 || ind >= curInd)
                throw new IndexOutOfRangeException();

            fixed (byte* ptr = _stack)
            {
                var defValue = (UnsafeDefValue<TDef, TValue>*)ptr;
                return defValue[ind];
            }
        }
    }

    //Stack Info
    public IEnumerable<TDef> Defs
    {
        get
        {
            for (var i = 0; i < curInd; i++)
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
        if (curInd == 0) return false;
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