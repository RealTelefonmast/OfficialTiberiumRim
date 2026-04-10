using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class WorldComponent_TiberiumMissions : WorldComponent
    {
        public List<Mission> Missions = new List<Mission>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref Missions, "Mission", LookMode.Deep);
            base.ExposeData();
        }

        public WorldComponent_TiberiumMissions(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            foreach (MissionDef missionDef in DefDatabase<MissionDef>.AllDefsListForReading)
            {
                if (missionDef.CanStartNow && !missionDef.IncidentBound)
                {
                    if (!Missions.Any((Mission x) => x.def == missionDef))
                    {
                        AddNewMission(missionDef);
                    }
                }
            }
            foreach(Mission mission in Missions)
            {
                foreach(MissionObjectiveDef objective in mission.def.objectives)
                {
                    if(objective.Active && !objective.IsFinished)
                    {
                        mission.TimePassed(objective, 1);
                    }
                }
            }
        }

        public void Notify_Seen(Mission mission)
        {
            if (!mission.seen)
            {
                Missions.Find((Mission x) => x == mission).seen = true;
            }
        }

        public void AddNewMission(MissionDef mission)
        {
            Mission newMission = new Mission(mission);
            Missions.Add(newMission);
        }
    }
}
