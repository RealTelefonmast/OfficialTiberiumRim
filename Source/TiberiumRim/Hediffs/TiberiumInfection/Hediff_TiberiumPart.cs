using Verse;

namespace TiberiumRim
{
    public class Hediff_TiberiumPart : Hediff_TRAddedPart
    {
        public bool addedManually = true;

        //TODO: Make visceral parts break down

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref addedManually, "addedManually");
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
            if (!pawn.IsHashIntervalTick(6000)) return;
            var risk = Risk;
            if (!TRUtils.Chance(risk))
            {
                addedManually = false;
                return;
            }
            pawn.health.RemoveHediff(this);
            BodyPartRecord part = Part.GetDirectChildParts().RandomElement();
            pawn.health.RestorePart(Part);
            HediffUtils.InfectPart(pawn, part, 0.1f * risk);
        }

        public bool IsRisky => addedManually;

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
