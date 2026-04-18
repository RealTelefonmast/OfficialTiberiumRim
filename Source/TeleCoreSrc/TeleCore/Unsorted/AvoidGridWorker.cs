using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class AvoidGridWorker
{
    protected AvoidGridDef def;
    protected byte[] grid;
    protected Verse.Map map;

    public AvoidGridWorker(Verse.Map map, AvoidGridDef def)
    {
        this.map = map;
        this.def = def;
        grid = new byte[map.cellIndices.NumGridCells];
    }

    public byte[] Grid => grid;

    public virtual bool AffectsThing(Thing thing)
    {
        return true;
    }

    public virtual void Notify_CellChanged(CellChangedEventArgs args)
    {
    }

    protected void SetAvoidValue(int index, byte value)
    {
        grid[index] = value;
    }

    protected void SetAvoidValue(IntVec3 cell, byte value)
    {
        grid[map.cellIndices.CellToIndex(cell)] = value;
    }
}