using System.Collections.Generic;
using TeleCore.RWExtended.ThingClasses;
using TR.Defs;
using Verse;

namespace TR.ThingData;

public class TRThingPrototype : TeleThing
{
    public new TRThingDef def;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        def = (TRThingDef)base.def;
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;
    }
}