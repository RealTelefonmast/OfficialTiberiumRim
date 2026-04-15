using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_TiberiumBath : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            //TODO: CHeck if this is necessary
            if(pawn.Downed || pawn.CarriedBy != null || pawn.Drafted || pawn.InAggroMentalState || pawn.InContainerEnclosed)
            {
                return null;
            }
            if (!pawn.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberAddHediff))
            {
                Need_Tiberium Need = (pawn.needs.AllNeeds.Find((Need x) => x is Need_Tiberium) as Need_Tiberium);
                if (Need != null && Need.CurCategory != TiberiumNeedCategory.Statisfied && !Need.IsInTiberium)
                {
                    TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Some, TraverseMode.PassDoors, false);
                    Thing targetA = TRUtils.ClosestPreferableReachableAndReservableTiberiumForHarvester(pawn, pawn.Position, pawn.Map, null, true, traverseParms, PathEndMode.OnCell);
                    Thing targetB = pawn.Map.listerThings.AllThings.Find((Thing x) => x != null && x.def.defName.Contains("TiberAdd") && pawn.CanReserve(x) && !x.IsForbidden(pawn) && pawn.CanReach(x.Position, PathEndMode.Touch, Danger.Some));
                    if (targetA != null || targetB != null)
                    {
                        JobDef job = DefDatabase<JobDef>.GetNamed("TiberiumBath");
                        return new Job(job, targetA, targetB);
                    }
                }
            }
            return null;
        }
    }

    public class JobDriver_TiberiumBath : JobDriver
    {
        private Toil bath;

        public override PawnPosture Posture
        {
            get
            {
                return (base.CurToil != this.bath) ? PawnPosture.Standing : PawnPosture.LayingAny;
            }
        }

        public LocalTargetInfo MainTarget
        {
            get
            {
                if (TargetA != null)
                {
                    return TargetA;
                }
                if(TargetB != null)
                {
                    return TargetB;
                }
                return null;
            }
        }

        public TargetIndex MainIndex
        {
            get
            {
                if (TargetA != null)
                {
                    return TargetIndex.A;
                }
                if (TargetB != null)
                {
                    return TargetIndex.B;
                }
                return TargetIndex.None;
            }
        }

        public override string GetReport()
        {
            if(MainTarget == TargetA)
            {
                return "TibRest".Translate();
            }
            return "TibConsume".Translate();
        }

        public override bool TryMakePreToilReservations()
        {
            if (pawn.CanReserve(MainTarget))
            {
                return this.pawn.Reserve(MainTarget, job);
            }
            return false;
        }

        private Need_Tiberium Need
        {
            get
            {
                return this.pawn.needs.AllNeeds.Find((Need x) => x is Need_Tiberium) as Need_Tiberium;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoCell(MainIndex, PathEndMode.OnCell);
            gotoToil.FailOnDespawnedOrNull(MainIndex);
            yield return gotoToil;

            if(MainIndex == TargetIndex.B)
            {
                yield return Toils_Ingest.ChewIngestible(pawn, 1f, TargetIndex.B);
                yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.B);
            }

            if (MainIndex == TargetIndex.A)
            {
                this.bath = new Toil
                {
                    tickAction = delegate
                    {
                        if (Need.CurCategory != TiberiumNeedCategory.Statisfied)
                        {
                            Need.CurLevel += 0.05f;
                        }
                    },
                };
                this.FailOnDespawnedOrNull(MainIndex);
                this.bath.FailOn(() => Need.CurCategory == TiberiumNeedCategory.Statisfied);
                yield return bath;
            }
        }

    }
}
