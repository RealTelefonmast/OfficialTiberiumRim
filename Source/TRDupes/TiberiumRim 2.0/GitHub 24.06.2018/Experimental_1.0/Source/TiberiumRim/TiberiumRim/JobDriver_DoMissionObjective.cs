using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_DoMissionObjective : WorkGiver_Scanner
    {
        private Mission Mission;

        private MissionObjectiveDef Objective;

        public WorldComponent_TiberiumMissions Missions
        {
            get
            {               
                return Find.World.GetComponent<WorldComponent_TiberiumMissions>();
            }
        }

        public override Job NonScanJob(Pawn pawn)
        {
            return base.NonScanJob(pawn);
        }

        public List<ThingDef> StationDefs
        {
            get
            {
                List<ThingDef> list = new List<ThingDef>();
                foreach(Mission mission in Missions.Missions)
                {
                    if (mission != null)
                    {
                        foreach (MissionObjectiveDef objective in mission.def.objectives)
                        {
                            foreach (ThingDef def in objective.stationDefs)
                            {
                                if (!list.Contains(def))
                                {
                                    list.Add(def);
                                }
                            }
                            if (objective.objectiveType == ObjectiveType.Examine)
                            {
                                foreach (ThingDef def in objective.targetThings)
                                {
                                    if (!list.Contains(def))
                                    {
                                        list.Add(def);
                                    }
                                }
                            }
                        }
                    }
                }
                return list;
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (!Missions.Missions.NullOrEmpty())
            {
                IEnumerable<Thing> things = pawn.Map.listerThings.AllThings.FindAll((Thing x) => StationDefs.Contains(x.def) && (x.TryGetComp<CompPowerTrader>()?.PowerOn ?? true));
                return things;
            }
            return null;
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn != null && t != null)
            {
                foreach (Mission mission in Missions.Missions)
                {
                    Mission = mission;
                    foreach (MissionObjectiveDef objective in mission.def.objectives)
                    {
                        if (objective.CanDoObjective(pawn, t))
                        {                          
                            Objective = objective;
                            this.def.verb = Objective.label;
                            this.def.workType.verb = Objective.label;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool Prioritized
        {
            get
            {
                return true;
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some))
            {
                JobInfo info = new JobInfo(Mission, null, null, Objective);
                return new JobWithObjects(TiberiumDefOf.DoMissionObjective, t, info);
            }
            return null;
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            if (Objective.EffectiveStatFactor != null)
            {
                return t.Thing.GetStatValue(Objective.EffectiveStatFactor, true);
            }
            return 1;
        }
    }

    public class JobDriver_DoMissionObjective : JobDriver
    {
        public JobWithObjects Job
        {
            get
            {
                return this.job as JobWithObjects;
            }
        }

        public Thing Station
        {
            get
            {
                return TargetA.Thing;
            }
        }

        public IntVec3 InteractionCell
        {
            get
            {
                if (Objective.distanceToTarget > 1)
                {
                    return Map.AllCells.Where(v => v.DistanceTo(Station.Position) == Objective.distanceToTarget).RandomElement();
                }
                return Station.InteractionCell;
            }
        }

        public Mission Mission
        {
            get
            {
                return Job.jobInfo.objectA as Mission;
            }
        }

        public MissionObjectiveDef Objective
        {
            get
            {
                return Job.jobInfo.defA as MissionObjectiveDef;
            }
        }

        public override string GetReport()
        {
            return Objective.LabelCap;
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(InteractionCell, PathEndMode.OnCell);

            Toil doObjective = new Toil();
            doObjective.tickAction = delegate
            {
                Pawn actor = doObjective.actor;

                float num = 1.1f;
                if (Objective.EffectiveStat != null && actor.SpecialDisplayStats.Any((StatDrawEntry x) => x.stat == Objective.EffectiveStat))
                {
                    num *= actor.GetStatValue(Objective.EffectiveStat, true);
                }
                if (Objective.EffectiveStatFactor != null)
                {
                    num *= this.TargetThingA.GetStatValue(Objective.EffectiveStatFactor, true);
                }
                foreach(SkillRequirement SR in Objective.skillRequirements)
                {
                    actor.skills.Learn(SR.skillDef, 0.11f, false);
                }
                Mission.WorkPerformed(Objective, num, actor);
                actor.GainComfortFromCellIfPossible();
            };
            doObjective.FailOn(() => Objective.IsFinished);
            doObjective.WithProgressBar(TargetIndex.A, () => Objective.ProgressPct, false, -0.5f);
            doObjective.defaultCompleteMode = ToilCompleteMode.Delay;
            doObjective.defaultDuration = 4000;
            yield return doObjective;
        }
    }
}
