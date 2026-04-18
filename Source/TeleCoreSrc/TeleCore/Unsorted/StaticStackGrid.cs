using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Verse;

namespace TeleCore.Unsorted;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct StaticValue<TDef, TValue>
    where TDef : Def
    where TValue : unmanaged
{
    private int _defID = 0;
    private Numeric<TValue> _value;
    private Numeric<TValue> _overflow;

    public int DefID => _defID;
    public TDef Def => DefID<TDef>.ToDef(_defID);

    public Numeric<TValue> Value
    {
        get => _value;
        set => _value = value;
    }

    public Numeric<TValue> Overflow
    {
        get => _overflow;
        set => _overflow = value;
    }

    public static implicit operator TValue(StaticValue<TDef, TValue> val) => val.Value;

    public static StaticValue<TDef, TValue> Invalid => new(0, Numeric<TValue>.Zero, Numeric<TValue>.Zero);
    public static StaticValue<TDef, TValue> Empty => new(ushort.MaxValue, Numeric<TValue>.Zero, Numeric<TValue>.Zero);

    public StaticValue(Def def, TValue value) : this(StaticData.ToID(def), value)
    {
    }

    public StaticValue(int defID, TValue value)
    {
        this._defID = defID;
        this._value = value;
    }

    public StaticValue(int defID, TValue value, TValue overflow)
    {
        this._defID = defID;
        this._value = value;
        this._overflow = overflow;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _defID.GetHashCode();
            hash = hash * 23 + _value.GetHashCode();
            hash = hash * 23 + _overflow.GetHashCode();
            return hash;
        }
    }

    #region Maths

    public static StaticValue<TDef, TValue> operator +(StaticValue<TDef, TValue> a, StaticValue<TDef, TValue> b)
    {
        a._value += b._value;
        a._overflow += b._overflow;
        return a;
    }

    public static StaticValue<TDef, TValue> operator -(StaticValue<TDef, TValue> a, StaticValue<TDef, TValue> b)
    {
        a._value -= b._value;
        a._overflow -= b._overflow;
        return a;
    }

    public static StaticValue<TDef, TValue> operator +(StaticValue<TDef, TValue> a, TValue b)
    {
        a._value += b;
        return a;
    }

    public static StaticValue<TDef, TValue> operator -(StaticValue<TDef, TValue> a, TValue b)
    {
        a._value -= b;
        return a;
    }

    #endregion

    public static bool operator ==(StaticValue<TDef, TValue> left, StaticValue<TDef, TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StaticValue<TDef, TValue> left, StaticValue<TDef, TValue> right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is not StaticValue<TDef, TValue> other) return false;
        return _defID == other._defID && _value == other._value && _overflow == other._overflow;
    }

    public override string ToString()
    {
        return $"[{DefID<TDef>.ToDef(_defID)}]: ({_value}, {_overflow})";
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct StaticStack<TDef, TValue> where TValue : unmanaged where TDef : Def
{
    private StaticValue<TDef, TValue>* stackPtr;
    private NativeArray<StaticValue<TDef, TValue>> stackData;
    private Numeric<TValue> totalValue;

    public bool HasAnyValue => totalValue > Numeric<TValue>.Zero;
    public int Length => stackData.Length;

    public StaticValue<TDef, TValue> this[TDef def] => this[StaticData.ToID(def)];

    public StaticValue<TDef, TValue> this[int idx]
    {
        get
        {
            if (idx > 0 || idx < stackData.Length)
                return stackPtr[idx];
            TLog.Warning($"Index out of bounds: {idx} [0...{Length}]");
            return StaticValue<TDef, TValue>.Empty;
        }
        //Main Setting Operation
        set
        {
            if (idx <= 0 && idx >= stackData.Length) return;
            AdjustTotalValue(stackPtr[idx].Value, value.Value);
            stackPtr[idx] = value;
        }
    }

    public StaticStack()
    {
        var allDefs = DefDatabase<TDef>.AllDefsListForReading;
        stackData = new NativeArray<StaticValue<TDef, TValue>>(allDefs.Count, Allocator.Persistent);
        stackPtr = (StaticValue<TDef, TValue>*)stackData.GetUnsafePtr();
        totalValue = Numeric<TValue>.Zero;

        for (var i = 0; i < allDefs.Count; i++)
        {
            stackPtr[i] = new StaticValue<TDef, TValue>(allDefs[i], Numeric<TValue>.Zero);
        }
    }

    private void AdjustTotalValue(Numeric<TValue> previousValue, Numeric<TValue> newValue)
    {
        totalValue = (TValue)(totalValue + (newValue - previousValue));
    }
}

public unsafe class StaticStackGrid<TDef, TValue>
    where TValue : unmanaged
    where TDef : Def
{
    private Verse.Map _map;
    private int _gridSize;
    private TDef[] _defArray;
    private int _defCount;

    private readonly NativeArray<StaticStack<TDef, TValue>> _grid;
    private readonly StaticStack<TDef, TValue>* _gridPtr;

    public StaticStackGrid(Verse.Map map)
    {
        _map = map;
        _gridSize = map.cellIndices.NumGridCells;
        _grid = new NativeArray<StaticStack<TDef, TValue>>(_gridSize, Allocator.Persistent); // new GasCellStack[gridSize];
        _gridPtr = (StaticStack<TDef, TValue>*)_grid.GetUnsafePtr();

        if (_defArray == null)
        {
            _defArray = DefDatabase<TDef>.AllDefsListForReading.ToArray();
            _defCount = _defArray.Length;
        }

        for (var c = 0; c < _gridSize; c++)
        {
            _gridPtr[c] = new StaticStack<TDef, TValue>();
        }
    }

    #region Public Safe Accessors

    public float DensityPercentAt(int index, int defID)
    {
        return (DensityAt(index, defID) / MaxDensityPerCellFor(DefID<TDef>.ToDef(defID))).AsPercent;
    }

    private Numeric<TValue> DensityAt(int index, int defID)
    {
        return _gridPtr[index][defID].Value;
    }

    public StaticStack<TDef, TValue> StackAt(int index)
    {
        return _gridPtr[index];
    }

    public Numeric<TValue> OverflowAt(int index, int defID)
    {
        return _gridPtr[index][defID].Overflow;
    }

    public bool AnyValueAt(IntVec3 cell)
    {
        return AnyGasAt(CellUtility.Index(cell, _map));
    }

    public bool AnyGasAt(int index)
    {
        return _gridPtr[index].HasAnyValue;
    }

    /// <summary>
    /// Public accessor to spawn gas.
    /// </summary>
    public void Notify_AddValue(IntVec3 cell, TDef type, Numeric<TValue> amount)
    {
        TryAddValueAt_Internal(cell, type, amount);
    }

    #endregion

    #region Unsafe Data Manipulation

    private void SetCellValueAt(int index, StaticValue<TDef, TValue> value)
    {
        //Set Value
        var val = _gridPtr[index];
        val[value.DefID] = value;
        _gridPtr[index] = val;
    }

    private void SetCellStackAt(int index, StaticStack<TDef, TValue> value)
    {
        for (var i = 0; i < _defCount; i++)
        {
            SetCellValueAt(index, value[i]);
        }
    }

    private void AdjustValue(ref StaticValue<TDef, TValue> cellValue, TDef def, Numeric<TValue> value, out Numeric<TValue> actualValue)
    {
        //TODO: This needs re-evaluation, is staticvalue necessary?
        actualValue = value.Value;
        /*actualValue = value;
        var val = cellValue + value;
        cellValue.Value = MathG.Clamp(val.Value, Numeric<TValue>.Zero, MaxDensityPerCellFor(def));
        if (val < Numeric<TValue>.Zero)
        {
            actualValue = value + val;
            return;
        }

        if (val < MaxDensityPerCellFor(def)) return;
        var overFlow = val - MaxDensityPerCellFor(def);
        actualValue = value - overFlow;
        cellValue.Overflow = (cellValue.Overflow + overFlow);*/
    }

    #endregion

    protected virtual Numeric<TValue> MaxDensityPerCellFor(TDef def)
    {
        return default;
    }

    protected virtual bool CanHaveValueAt(int index, TDef type)
    {
        return true;
    }

    private void TryAddValueAt_Internal(IntVec3 cell, TDef type, Numeric<TValue> amount, bool noOverflow = false)
    {
        if (!CanHaveValueAt(CellUtility.Index(cell, _map), type)) return;

        int index = CellIndicesUtility.CellToIndex(cell, _map.Size.x);
        var cellValue = _gridPtr[index][type];
        AdjustValue(ref cellValue, type, amount, out _);

        if (noOverflow)
            cellValue.Overflow = Numeric<TValue>.Zero;

        SetCellValueAt(index, cellValue);
    }

}