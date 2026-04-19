using Verse;

namespace TiberiumRim;

public class TiberiumEntityGrid : BoolGrid
{
    public Map map;
    public TiberiumEntity[] TiberiumEntities;

    public TiberiumEntityGrid(Map map) : base(map)
    {
        this.map = map;
        TiberiumEntities = new TiberiumEntity[map.cellIndices.NumGridCells];
    }

    public void Tick()
    {
        foreach (var entity in TiberiumEntities) entity.Tick();
    }

    public void Set(int index, bool value, TiberiumEntity entity)
    {
        base.Set(index, value);
        TiberiumEntities[index] = entity;
    }

    public void Set(IntVec3 c, bool value, TiberiumEntity entity)
    {
        base.Set(c, value);
        TiberiumEntities[map.cellIndices.CellToIndex(c)] = entity;
    }
}