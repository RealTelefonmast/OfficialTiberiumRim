using UnityEngine;
using Verse;

namespace TR.Grids;

public class TiberiumFloraGrid : ICellBoolGiver
{
    public CellBoolDrawer drawer;

    public BoolGrid floraBools;
    public BoolGrid growBools;
    public Map map;

    public TiberiumFloraGrid(Map map)
    {
        this.map = map;
        floraBools = new BoolGrid(map);
        growBools = new BoolGrid(map);
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
    }

    //Bool Getters
    public bool GetCellBool(int index)
    {
        return growBools[index];
    }

    public Color Color => Color.white;

    public Color GetCellExtraColor(int index)
    {
        if (growBools[index]) return Color.green;
        return Color.red;
    }

    //
    public void SetFlora(IntVec3 c, bool value)
    {
        floraBools.Set(c, value);
    }

    public void SetGrow(IntVec3 c, bool value)
    {
        growBools.Set(c, true);
    }

    public void Notify_PlantSpawned(TiberiumPlant plant)
    {
    }
}