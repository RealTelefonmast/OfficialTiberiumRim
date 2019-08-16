using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Hediff_TiberiumPart : Hediff_ReplacedPart
    {
        public bool addedManually = false;
        public bool mutated = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref addedManually, "addedManually");
            Scribe_Values.Look(ref mutated, "mutated");
        }

        public override string LabelInBrackets
        {
            get
            {
                if (IsRisky)
                    return "Risk".Translate() + " " + Risk.ToStringPercent();
                return base.LabelInBrackets;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (!IsRisky) return;
            if (pawn.IsHashIntervalTick(6000))
            {
                var risk = Risk;
                if (!TRUtils.Chance(risk)) return;

                pawn.health.RemoveHediff(this);
                BodyPartRecord part = Part.GetDirectChildParts().RandomElement();
                pawn.health.RestorePart(Part);
                HediffUtils.TryInfect(pawn, part, 0.1f * risk);
            }
        }

        public bool IsRisky => addedManually && !mutated;

        private float Risk
        {
            get
            {
                float num = 0f;
                num += 1 - pawn.Health();
                if (pawn.Position.GetTiberium(pawn.Map) != null)
                    num += 0.1f;
                return num;
            }
        }
    }
}
