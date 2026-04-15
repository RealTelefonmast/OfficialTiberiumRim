using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class IncidentWorker_MissionStart : IncidentWorker
    {
        public MissionIncidentDef Def
        {
            get
            {
                return this.def as MissionIncidentDef;
            }
        }

        public WorldComponent_TiberiumMissions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_TiberiumMissions>();
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            foreach (MissionDef mission in Def.missionUnlocks)
            {
                Missions.AddNewMission(mission);
            }
            return true;

        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            if (!Def.CanStart(Find.Maps.Find((Map x) => x.Tile == target.Tile)))
            {
                return false;
            }
            return true;
        }
    }
}
