using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class MissionIncidentDef : IncidentDef
    {
        public List<JobDef> anyJobNeeded = new List<JobDef>();

        public List<ThingDef> thingRequisites = new List<ThingDef>();

        public List<MissionDef> missionRequisites = new List<MissionDef>();

        public List<MissionObjectiveDef> objectiveRequisites = new List<MissionObjectiveDef>();

        public List<MissionDef> missionUnlocks;

        public WorldComponent_TiberiumMissions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_TiberiumMissions>();
            }
        }

        public bool ObjectivesDone
        {
            get
            {
                if (!objectiveRequisites.NullOrEmpty())
                {
                    return objectiveRequisites.All((MissionObjectiveDef x) => x.IsFinished);
                }
                return true;
            }
        }

        public bool MissionsDone
        {
            get
            {
                if (!missionRequisites.NullOrEmpty())
                {
                    return Missions.Missions.All((Mission x) => missionRequisites.Contains(x.def) && x.def.IsFinished);
                }
                return true;
            }
        }

        public bool ThingsAvailable(Map map)
        {
            if (!thingRequisites.NullOrEmpty())
            {
                return map.listerThings.AllThings.All((Thing x) => thingRequisites.Contains(x.def));
            }
            return true;
        }

        public bool JobsAvailable(Map map)
        {
            if (!anyJobNeeded.NullOrEmpty())
            {
                Pawn pawn = map.mapPawns.AllPawns.Where((Pawn x) => x.IsColonist && anyJobNeeded.Contains(x.CurJob.def))?.RandomElement();
                if (pawn == null)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanStart(Map map)
        {
            if (Missions.Missions.All((Mission x) => !missionUnlocks.Contains(x.def)))
            {
                if(ObjectivesDone && MissionsDone && ThingsAvailable(map) && JobsAvailable(map))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
