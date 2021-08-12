using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Need_Tiberium : Need
    {
        public Need_Tiberium(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>
            {
                0.15f, // Urgent
                0.50f // Lacking
            };
        }

        public enum TiberiumNeedCategory
        {
            Statisfied,
            Lacking,
            Urgent
        }

        public override int GUIChangeArrow => IsBeingSatisfied ? 1 : -1;

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = Rand.Range(0.2f, 0.5f);
        }

        public override void NeedInterval()
        {
            if (pawn.SpawnedOrAnyParentSpawned)
            {
                if (pawn.CarriedBy == null && !IsBeingSatisfied)
                    CurLevel -= TiberiumNeedFallPerTick * 350;
            }
        }

        public TiberiumNeedCategory CurCategory
        {
            get
            {
                if (this.CurLevel <= 0.15f)
                {
                    return TiberiumNeedCategory.Urgent;
                }
                if (this.CurLevel < 0.50f)
                {
                    return TiberiumNeedCategory.Lacking;
                }
                return TiberiumNeedCategory.Statisfied;
            }
        }

        private float TiberiumNeedFallPerTick => this.def.fallPerDay / 60000f;

        private bool IsBeingSatisfied => IsInTiberium || HasTiberAdd;

        public bool HasTiberAdd => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberAddHediff);

        private bool IsInTiberium
        {
            get
            {
                if (pawn.Spawned)
                    return pawn.Position.GetTiberium(pawn.Map) != null;
                return false;
            }
        }
    }
}
