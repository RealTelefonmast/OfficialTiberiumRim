using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using StoryFramework;
using Verse;

namespace TiberiumRim
{
    public class Incident_Skyfaller : IncidentWorker
    {
        private List<Skyfaller> skyfallers = new List<Skyfaller>();
        private List<IntVec3> positions = new List<IntVec3>();

        public TiberiumIncidentDef Def => base.def as TiberiumIncidentDef;

        protected void Prepare(Map map)
        {
            ThingSkyfaller pair = Def.skyfallers.RandomWeightedElement(s => s.chance);
            for (int i = 0; i < pair.amount; i++)
            {
                skyfallers.Add(SkyfallerMaker.MakeSkyfaller(pair.skyfallerDef, pair.innerThing));
            }
            positions = Def.positions.FindCells(map, pair.amount, null, skyfallers.Select(s => s.innerContainer[0].def).ToList());

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
    }
}
