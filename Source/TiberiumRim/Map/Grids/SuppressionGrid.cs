using System.Collections.Generic;
using System.Linq;
using TR.Comps;
using Verse;

namespace TR.Grids;

public class SuppressionGrid
{
    public BoolGrid CoveredBools;
    public Map map;
    public List<Comp_Suppression> Sources = new();
    public BoolGrid SuppressionBools;

    public SuppressionGrid(Map map)
    {
        this.map = map;
        CoveredBools = new BoolGrid(map);
        SuppressionBools = new BoolGrid(map);
    }

    public List<Comp_Suppression> SuppressorsAt(IntVec3 cell, Comp_Suppression except = null)
    {
        return Sources.Where(s => s != except && s.CoversCell(cell)).ToList();
    }

    public void RegisterSource(Comp_Suppression source)
    {
        Sources.Add(source);
    }

    public void DeregisterSource(Comp_Suppression source)
    {
        Sources.Remove(source);
        foreach (var cell in source.SuppressionCells)
        {
            if (!cell.InBounds(map)) continue;
            CoveredBools[cell] = !SuppressorsAt(cell, source).NullOrEmpty();
        }

        ToggleSuppressor(source, false);
    }

    public void Notify_SuppressionSourceUpdated(Comp_Suppression suppressor, List<IntVec3> oldCells)
    {
        foreach (var cell in oldCells)
        {
            if (!cell.InBounds(map)) continue;
            CoveredBools[cell] = false;
            SuppressionBools[cell] = Enumerable.Any(SuppressorsAt(cell, suppressor), t => t.AffectsCell(cell));
        }

        foreach (var cell in suppressor.SuppressionCells)
        {
            if (!cell.InBounds(map)) continue;
            CoveredBools[cell] = true;
        }
    }

    public void ToggleSuppressor(Comp_Suppression suppressor, bool toggleOn)
    {
        foreach (var cell in suppressor.SuppressionCells)
            SuppressionBools[cell] = Enumerable.Any(SuppressorsAt(cell, suppressor), s => s.SuppressingNow) ||
                                     (suppressor.SuppressingNow && toggleOn);
    }
}