using TR.Grids;
using TR.TiberiumObjects;
using UnityEngine;
using Verse;

namespace TR.Info;

public class TiberiumTerrainInfo : MapInformation
{
    private TiberiumTerrainGrid terrainGrid;
    public TiberiumWaterInfo WaterInfo;

    public TiberiumTerrainInfo(Map map) : base(map)
    {
        terrainGrid = new TiberiumTerrainGrid(map);
        WaterInfo = new TiberiumWaterInfo(map);
    }

    public override void ExposeDataExtra()
    {
        Scribe_Deep.Look(ref terrainGrid, "terrainGrid");
        Scribe_Deep.Look(ref WaterInfo, "WaterInfo");
    }

    public override void InfoInit(bool initAfterReload = false)
    {
        base.InfoInit(initAfterReload);
        WaterInfo.InfoInit(initAfterReload);
    }

    public override void Tick()
    {
        WaterInfo.Tick();
    }

    public override void Update()
    {
        WaterInfo.Update();
    }

    public void Notify_TibSpawned(TiberiumCrystal crystal)
    {
        WaterInfo.Notify_TibSpawned(crystal);
    }
}

public class ColorOffsetGrid : ICellBoolGiver
{
    public Color[] colorGrid;
    public Map map;

    public ColorOffsetGrid(Map map)
    {
        this.map = map;
        colorGrid = new Color[map.cellIndices.NumGridCells];
    }

    public bool GetCellBool(int index)
    {
        return true;
    }

    public Color GetCellExtraColor(int index)
    {
        return Color.cyan;
    }

    public Color Color => Color.cyan;
}