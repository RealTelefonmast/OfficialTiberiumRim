// Preserved from TeleCore/SpreadingGas/GasCellStack.cs

using System.Runtime.InteropServices;
using Unity.Collections;
using Verse;

namespace TeleCore.Types.Structs;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct GasCellStack_TAE
{
    internal TAE.GasCellValue* stackPtr;
    private NativeArray<TAE.GasCellValue> stackData;
    internal uint totalValue;

    public bool HasAnyGas => totalValue > 0;
    public int Length => stackData.Length;

    public TAE.GasCellValue this[TAE.SpreadingGasTypeDef def] => this[def.IDReference];

    public TAE.GasCellValue this[int idx]
    {
        get
        {
            if (idx > 0 || idx < stackData.Length)
                return stackPtr[idx];
            return TAE.GasCellValue.Empty;
        }
        set
        {
            if (idx <= 0 && idx >= stackData.Length) return;
            ChangedValueOf(stackPtr[idx].value, value.value);
            stackPtr[idx] = value;
        }
    }

    public GasCellStack_TAE()
    {
        var allDefs = DefDatabase<TAE.SpreadingGasTypeDef>.AllDefsListForReading;
        stackData = new NativeArray<TAE.GasCellValue>(allDefs.Count, Allocator.Persistent);
        stackPtr = (TAE.GasCellValue*)stackData.GetUnsafePtr();
        totalValue = 0;

        for (var i = 0; i < TAE.SpreadingGasGrid.GasDefsCount; i++) stackPtr[i] = new TAE.GasCellValue(allDefs[i], 0);
    }

    public GasCellStack_TAE(NativeArray<TAE.GasCellValue> stackData)
    {
        this.stackData = stackData;
        stackPtr = (TAE.GasCellValue*)stackData.GetUnsafePtr();
        totalValue = 0;
        totalValue = (uint)stackData.Sum(c => c.value);
    }

    private void ChangedValueOf(int diff)
    {
        totalValue = (uint)(totalValue + diff);
    }

    private void ChangedValueOf(ushort previousValue, ushort newValue)
    {
        totalValue = (uint)(totalValue + (newValue - previousValue));
    }
}