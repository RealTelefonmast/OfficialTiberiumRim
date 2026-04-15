using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse.AI;
using Verse;

namespace TiberiumRim
{
    public class JobGiver_Amalgamation : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Visceroid visceroid = pawn as Visceroid;
            if(visceroid.OtherVisceroidExists(out Visceroid visc))
            {
                float chance = MainTCD.MainTiberiumControlDef.AmalgamationChance * pawn.Map.listerThings.AllThings.FindAll((Thing x) => x is Visceroid).Count;
                if (Rand.Chance(chance))
                {
                    JobDef job = DefDatabase<JobDef>.GetNamed("FormAmalgamation");
                    return new Job(job, visc);
                }
            }
            return null;
        }
    }

    public class JobDriver_Amalgamation : JobDriver
    {
        protected Visceroid VisceroidMain
        {
            get
            {
                return (Visceroid)this.job.targetA.Thing;
            }
        }

        protected Visceroid VisceroidTwo
        {
            get
            {
                return this.pawn as Visceroid;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            if (pawn.CanReserve(this.TargetA))
            {
                return this.pawn.Reserve(this.TargetA, this.job);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil FormAmal = Toils_General.WaitWith(TargetIndex.A, 750, true, true);
            yield return Toils_General.Do(delegate
            {
                IntVec3 pos = VisceroidMain.Position;
                Map map = VisceroidMain.Map;
                VisceroidMain.DeSpawn();
                VisceroidTwo.DeSpawn();
                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named("Amalgamation_TBI"));
                pawn = PawnGenerator.GeneratePawn(request);
                pawn.health = new Pawn_HealthTracker(pawn);
                pawn.ageTracker.AgeBiologicalTicks = 0;
                pawn.ageTracker.AgeChronologicalTicks = 0;
                GenSpawn.Spawn(pawn, pos, map);
            });
            yield break;
        }
    }
}
