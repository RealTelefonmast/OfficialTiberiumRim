using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class TiberiumGarden
{
    private List<IntVec3> cells = new();
    private IntVec3 center;
    private TiberiumFloraGrid floraGrid;
    private Map map;


    public TiberiumGarden(TiberiumFloraGrid floraGrid)
    {
        this.floraGrid = floraGrid;
        map = floraGrid.map;
    }

    public void GardenTick()
    {
    }

    private void CalculateCenter()
    {
    }

    public void AddCell(IntVec3 cell)
    {
    }
}