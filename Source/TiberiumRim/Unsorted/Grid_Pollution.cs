using UnityEngine;
using Verse;

namespace TiberiumRim;

public class PollutionGrid : ICellBoolGiver, IExposable
{
    public CellBoolDrawer Drawer;
    public ushort[] Grid;
    public Map map;

    public Color Color => new();

    public Color GetCellExtraColor(int index)
    {
        return new Color();
    }

    public bool GetCellBool(int index)
    {
        return true;
    }

    public void ExposeData()
    {
    }
}