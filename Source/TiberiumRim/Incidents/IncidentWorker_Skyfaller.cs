using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TR.Incidents;

public class IncidentWorker_Skyfaller : IncidentWorker_TR
{
    private readonly List<Skyfaller> skyfallers = new();
    private List<IntVec3> positions = new();

    public TiberiumIncidentDef Def => def as TiberiumIncidentDef;

    private void Prepare(Map map)
    {
        var pair = Def.skyfallers.RandomWeightedElement(s => s.chance);
        for (var i = 0; i < pair.amount; i++)
            skyfallers.Add(SkyfallerMaker.MakeSkyfaller(pair.skyfallerDef, pair.innerThing));
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

    //TODO: Implement positionfilter correctly / neatly
    /*
    protected void Prepare(Map map)
    {
        SkyfallerValue pair = def.skyfallers.RandomWeightedElement(s => s.chance);
        positions = def.positionFilter.NeededCellsFor(map, def.skyfallers.Select(t => t.innerThing).ToList()).ToList();
        for (int i = 0; i < pair.amount; i++)
        {
            skyfallers.Add(SkyfallerMaker.MakeSkyfaller(pair.skyfallerDef, pair.innerThing));
        }
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
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
    */
}