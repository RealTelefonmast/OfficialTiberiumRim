using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace TiberiumRim
{
    public class MissionObjectiveDef : Def
    {
        public List<ThingDef> targetThings = new List<ThingDef>();

        public List<ThingDef> stationDefs = new List<ThingDef>();

        public List<ThingDef> rewards = new List<ThingDef>();

        public List<PawnKindDef> targetPawns = new List<PawnKindDef>();

        public float distanceToTarget = 0;

        public RewardType rewardType = RewardType.None;

        public ObjectiveType objectiveType = ObjectiveType.None;

        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();

        public List<MissionObjectiveDef> objectiveRequisites = new List<MissionObjectiveDef>();

        public List<MissionObjectiveDef> dependantOn = new List<MissionObjectiveDef>();

        public List<IncidentProperties> incidentsOnCompletion = new List<IncidentProperties>();

        public List<IncidentProperties> incidentsOnFail = new List<IncidentProperties>();

        public int workCost = 0;
        
        public float timerDays = 0;

        public List<string> images = new List<string>();

        public override IEnumerable<string> ConfigErrors()
        {
            if(objectiveType == ObjectiveType.None)
            {
                Log.Error("Missing objective type for " + this.defName);
            }
            if(objectiveType == ObjectiveType.Wait && timerDays <= 0f)
            {
                yield return "Objective of type 'Wait' is missing a timer value.";
            }
            if(((objectiveType == ObjectiveType.Discover || objectiveType == ObjectiveType.Craft || objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Destroy) && targetThings.NullOrEmpty()) || (objectiveType == ObjectiveType.Hunt && targetPawns.NullOrEmpty()))
            {
                yield return "Objective is missing a target.";
            }
            if(objectiveType == ObjectiveType.Examine && workCost <= 0)
            {
                yield return "Objective of type 'Examine' is missing a work value.";
            }
            if (objectiveType == ObjectiveType.Examine && (targetThings.NullOrEmpty() || targetPawns.NullOrEmpty()))
            {
                yield return "Objective of type 'Examine' is missing a target.";
            }
            if (objectiveType == ObjectiveType.Research && workCost <= 0)
            {
                yield return "Objective of type 'Research' is missing a work value.";
            }
            if (objectiveType == ObjectiveType.Research && stationDefs.NullOrEmpty())
            {
                yield return "Objective of type 'Research' is missing at least one defined work station.";
            }
        }

        public bool Active
        {
            get
            {
                if (GetTimer == TimerTicks)
                {
                    if (!objectiveRequisites.NullOrEmpty())
                    {
                        return objectiveRequisites.All((MissionObjectiveDef x) => x.IsFinished);
                    }
                }
                return true;
            }
        }

        public bool IsManualJob
        {
            get
            {
                if(objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Craft || objectiveType == ObjectiveType.Destroy || objectiveType == ObjectiveType.Wait)
                {
                    return true;
                }
                return false;
            }
        }

        public bool CanDoObjective(Pawn pawn, Thing thing)
        {
            if (!Active || IsManualJob || Failed || IsFinished || pawn == null)
            {
                return false;
            }
            if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
            {
                return false;
            }
            if (!stationDefs.NullOrEmpty() && thing != null && !stationDefs.Contains(thing.def))
            {
                return false;
            }
            if (!skillRequirements.All(sr => pawn.skills.skills.Any(sr2 => sr2.def == sr.skillDef && sr2.levelInt >= sr.skillLevel)))
            {
                return false;
            }
            if(objectiveType == ObjectiveType.Hunt)
            {
                if (!Find.AnyPlayerHomeMap.mapPawns.AllPawnsSpawned.Any(x => targetPawns.Contains(x.kindDef)))
                {
                    return false;
                }
            }
            if(objectiveType == ObjectiveType.Examine)
            {
                if (!targetThings.NullOrEmpty() && thing != null && !targetThings.Contains(thing.def))
                {
                    return false;
                }
            }
            if(objectiveType == ObjectiveType.Examine)
            {

                if (!Find.AnyPlayerHomeMap.listerThings.AllThings.Any((Thing x) => targetThings.Contains(x.def)))
                {
                    return false;
                }
            }
            return true;
        }

        public ThingDef BestPotentialStationDef
        {
            get
            {
                float num = 0f;
                foreach(ThingDef def in stationDefs)
                {
                    float num2 = def.statBases.Sum(s => s.value);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                return stationDefs.Find(x => x.statBases.Sum(s => s.value) == num);
            }
        }

        public StatDef EffectiveStat
        {
            get
            {
                switch(objectiveType)
                {
                    case ObjectiveType.Construct:
                        return StatDefOf.ConstructionSpeed;
                    case ObjectiveType.Craft:
                        return StatDefOf.WorkSpeedGlobal;
                    case ObjectiveType.Examine:
                        return StatDefOf.ResearchSpeed;
                    case ObjectiveType.Research:
                        return StatDefOf.ResearchSpeed;
                }
                return null;
            }
        }

        public int TimerTicks
        {
            get
            {
                return Mathf.RoundToInt(GenDate.TicksPerDay * timerDays);
            }
        }

        public StatDef EffectiveStatFactor
        {
            get
            {
                switch (objectiveType)
                {
                    case ObjectiveType.Craft:
                        return StatDefOf.WorkTableWorkSpeedFactor;
                    case ObjectiveType.Research:
                        return StatDefOf.ResearchSpeedFactor;
                }
                return null;
            }
        }

        public bool IsFinished
        {
            get
            {
                if(workCost > 0)
                {
                    if(Progress >= workCost)
                    {
                        return true;
                    }
                }
                if(objectiveType == ObjectiveType.Construct || objectiveType == ObjectiveType.Craft)
                {
                    if(targetThings.All((ThingDef def) => Find.AnyPlayerHomeMap.listerThings.AllThings.Any(x => x.def == def)))
                    {
                        return true;
                    };
                }
                if(objectiveType == ObjectiveType.Wait)
                {
                    return GetTimer == 0;
                }
                if(!dependantOn.NullOrEmpty() && dependantOn.All(m => m.IsFinished))
                {
                    return true;
                }
                return false;
            }
        }

        public int GetTimer
        {
            get
            {
                return Missions.Missions.Find((Mission x) => x.def.objectives.Contains(this)).GetTimer(this);
            }
        }

        public bool Failed
        {
            get
            {
                if(TimerTicks > 0 && objectiveType != ObjectiveType.Wait && GetTimer <= 0)
                {
                    return true;
                }
                return false;
            }
        }

        public float Progress
        {
            get
            {
                Mission mission = Find.World.GetComponent<WorldComponent_TiberiumMissions>().Missions.Find((Mission x) => x.objectiveProgress.Keys.Contains(this));
                if(mission != null)
                {
                   return mission.objectiveProgress[this];
                }
                return 0f;
            }
        }

        public float ProgressPct
        {
            get
            {
               return Progress / workCost;
            }
        }

        public WorldComponent_TiberiumMissions Missions
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_TiberiumMissions>();
            }
        }

        public Mission GetMission()
        {
            return Missions.Missions.Find((Mission x) => x.def.objectives.Contains(this));
        }
    }

    public enum ObjectiveType
    {
        Research,
        Discover,
        Destroy,
        Examine,
        Construct,
        Craft,
        Wait,
        Hunt,
        None
    }

    public enum RewardType
    {
        DropPods,
        SpawnOnObjective,
        None
    }
}
