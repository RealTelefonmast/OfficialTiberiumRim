using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IncidentWorker_Skyfaller : IncidentWorker_TR
    {
        private List<Skyfaller> skyfallers = new List<Skyfaller>();
        private List<IntVec3> positions = new List<IntVec3>();

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            throw new NotImplementedException("Skyfaller IncidentWorker is not working yet!");
        }

        //TODO: Implement positionfilter correctly / neatly
        /*
        protected void Prepare(Map map)
        {
            SkyfallerValue pair = props.skyfallers.RandomWeightedElement(s => s.chance);
            positions = props.positionFilter.NeededCellsFor(map, props.skyfallers.Select(t => t.innerThing).ToList()).ToList();
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
}
