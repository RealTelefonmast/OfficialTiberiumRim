using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TR;

public class Incident_Skyfaller : IncidentWorker
{
    private readonly List<Skyfaller> skyfallers = new();
    private List<IntVec3> positions = new();

    public TiberiumIncidentDef Def => def as TiberiumIncidentDef;

    private void Prepare(Map map)
    {
        var pair = Def.skyfallers.RandomWeightedElement(s => s.chance);
        for (var i = 0; i < pair.amount; i++)
            skyfallers.Add(SkyfallerMaker.MakeSkyfaller((ThingDef)pair.skyfallerDef, (ThingDef)pair.innerThing));
        positions = Def.positions.FindCells(map, pair.amount, null,
            skyfallers.Select(s => s.innerContainer[0].def).ToList());
    }

    public override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms)) return false;
        Prepare(parms.target as Map);
        for (var i = 0; i < positions.Count; i++)
        {
            var pos = positions[i];
            GenSpawn.Spawn(skyfallers[i], pos, parms.target as Map);
        }

        return true;
    }
}