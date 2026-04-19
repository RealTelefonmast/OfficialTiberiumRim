using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim;

public class TNW_TNC : TiberiumNetworkBuilding
{
    public override IEnumerable<IntVec3> ConnectableCells
    {
        get
        {
            var rect = this.OccupiedRect();
            var cells = rect.Cells.ToList();
            rect.Corners.ToList().ForEach(x => cells.Remove(x));
            return cells;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        //MainPipeNetwork = new TiberiumNetwork(this, Manager);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.GetInspectString().TrimEndNewlines());
        if (DebugSettings.godMode) sb.AppendLine("Connected Structures: " + Network.NetworkSet.FullList.Count);
        return sb.ToString().TrimEndNewlines();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        yield return new Designator_PlacePipe(this);
    }
}