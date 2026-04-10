using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld;
using Verse;
using Verse.AI;

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

        public override int GUIChangeArrow
        {
            get
            {
                if (IsInTiberium || HasTiberAdd)
                {
                    return 1;
                }
                return -1;
            }
        }

        public TiberiumNeedCategory CurCategory
        {
            get
            {
                if (this.CurLevel >= 0.50f)
                {
                    return TiberiumNeedCategory.Statisfied;
                }
                if (this.CurLevel <= 0.50f)
                {
                    return TiberiumNeedCategory.Lacking;
                }
                if (this.CurLevel <= 0.15f)
                {
                    return TiberiumNeedCategory.Urgent;
                }
                return 0;
            }
        }

        public override float CurLevel
        {
            get
            {
                return base.CurLevel;
            }
            set
            {
                TiberiumNeedCategory curCategory = this.CurCategory;
                base.CurLevel = value;
            }
        }

        public bool IsInTiberium
        {
            get
            {
                IntVec3 c = this.pawn.Position;
                if (c.InBounds(this.pawn.Map))
                {
                    TiberiumCrystal crystal = c.GetTiberium(this.pawn.Map);
                    if (crystal != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasTiberAdd
        {
            get
            {
                return this.pawn.health.hediffSet.HasHediff(TiberiumHediffDefOf.TiberAddHediff);
            }
        }

        private float TiberiumNeedFallPerTick
        {
            get
            {
                return this.def.fallPerDay / 60000f;
            }
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = Rand.Range(0.2f, 0.5f);
        }

        public override void NeedInterval()
        {
            if (this.pawn?.Map != null)
            {
                if (this.pawn.Position.InBounds(this.pawn.Map))
                {
                    if (this.pawn.CarriedBy != null)
                    {
                        return;
                    }
                    if (!IsInTiberium)
                    {
                        this.CurLevel -= this.TiberiumNeedFallPerTick * 350f;
                    }
                }
            }
        }
    }

    public enum TiberiumNeedCategory : byte
    {
        Statisfied,
        Lacking,
        Urgent
    }
}

