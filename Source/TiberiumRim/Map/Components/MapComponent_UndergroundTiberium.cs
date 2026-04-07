using Verse;

namespace TR.Components;

public class MapComponent_UndergroundTiberium : MapComponent
{
    public ushort[] crystalGrid;
    public ushort[] gasGrid;

    public MapComponent_UndergroundTiberium(Map map) : base(map)
    {
        gasGrid = new ushort[map.cellIndices.NumGridCells];
    }

    public void CreateGasDepot()
    {
    }
}