using UnityEngine;
using Verse;

namespace TiberiumRim;

public class SuppressionGrid : ICellBoolGiver
{
    public CellBoolDrawer drawer;
    public Map map;
    public BoolGrid suppressionBools;

    public SuppressionGrid(Map map)
    {
        this.map = map;
        suppressionBools = new BoolGrid(map);
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.4f);
    }

    public Color Color => Color.white;

    public Color GetCellExtraColor(int index)
    {
        return suppressionBools[index] ? Color.red : Color.gray;
    }

    public bool GetCellBool(int index)
    {
        return suppressionBools[index];
    }

    public void Set(IntVec3 c, bool value)
    {
        suppressionBools.Set(c, value);
    }
}