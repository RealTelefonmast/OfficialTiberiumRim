using System.Linq;
using System.Runtime.InteropServices;
using TeleCore.Defs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Verse;

namespace TeleCore.Unsorted;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct GasCellStack
{
    internal GasCellValue* stackPtr;
    private NativeArray<GasCellValue> stackData;
    internal uint totalValue;

    public bool HasAnyGas => totalValue > 0;
    public int Length => stackData.Length;

    public GasCellValue this[SpreadingGasTypeDef def] => this[def.IDReference];

    public GasCellValue this[int idx]
    {
        get
        {
            if (idx > 0 || idx < stackData.Length)
                return stackPtr[idx];
            return GasCellValue.Empty;
        }
        //Main Setting Operation
        set
        {
            if (idx <= 0 && idx >= stackData.Length) return;
            ChangedValueOf(stackPtr[idx].value, value.value);
            stackPtr[idx] = value;
        }
    }

    public GasCellStack()
    {
        var allDefs = SpreadingGasGrid.GasDefsArr; //DefDatabase<SpreadingGasTypeDef>.AllDefsListForReading;
        stackData = new NativeArray<GasCellValue>(allDefs.Length, Allocator.Persistent);
        stackPtr = (GasCellValue*)stackData.GetUnsafePtr();
        totalValue = 0;

        //
        for (var i = 0; i < SpreadingGasGrid.GasDefsCount; i++) stackPtr[i] = new GasCellValue(allDefs[i], 0);
    }

    public GasCellStack(NativeArray<GasCellValue> stackData)
    {
        this.stackData = stackData;
        stackPtr = (GasCellValue*)stackData.GetUnsafePtr();
        totalValue = 0;

        totalValue = (uint)stackData.Sum(c => c.value);
    }


    //Numerically
    public static GasCellStack operator +(GasCellStack stack, (SpreadingGasTypeDef def, ushort val) value)
    {
        stack[value.def.IDReference] += new GasCellValue(value.def.IDReference, value.val);
        stack.ChangedValueOf(value.val);
        return stack;
    }

    public static GasCellStack operator -(GasCellStack stack, (SpreadingGasTypeDef def, ushort val) value)
    {
        stack[value.def.IDReference] -= new GasCellValue(value.def.IDReference, value.val);
        stack.ChangedValueOf(-value.val);
        return stack;
    }

    //CellValue
    public static GasCellStack operator +(GasCellStack stack, GasCellValue value)
    {
        stack[value.defID] += value;
        stack.ChangedValueOf(value.value);
        return stack;
    }

    public static GasCellStack operator -(GasCellStack stack, GasCellValue value)
    {
        stack[value.defID] -= value;
        stack.ChangedValueOf(-value.value);
        return stack;
    }

    //Stacks
    public static GasCellStack operator +(GasCellStack stack, GasCellStack value)
    {
        for (var i = 0; i < stack.Length; i++)
        {
            stack[i] += value[i];
            stack.ChangedValueOf(value[i].value);
        }

        return stack;
    }

    public static GasCellStack operator -(GasCellStack stack, GasCellStack value)
    {
        for (var i = 0; i < stack.Length; i++)
        {
            stack[i] -= value[i];
            stack.ChangedValueOf(-value[i].value);
        }

        return stack;
    }

    private void ChangedValueOf(int diff)
    {
        totalValue = (uint)(totalValue + diff);
    }

    private void ChangedValueOf(ushort previousValue, ushort newValue)
    {
        totalValue = (uint)(totalValue + (newValue - previousValue));
    }

    //Static Values
    public static GasCellStack Max => new(FullStack);

    private static NativeArray<GasCellValue> FullStack
    {
        get
        {
            var list = DefDatabase<SpreadingGasTypeDef>.AllDefsListForReading;
            var stack = new NativeArray<GasCellValue>(list.Count,
                Allocator.Persistent); // new GasCellValue[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var gas = list[i];
                stack[i] = new GasCellValue(gas.IDReference, (ushort)gas.maxDensityPerCell);
            }

            return stack;
        }
    }
}