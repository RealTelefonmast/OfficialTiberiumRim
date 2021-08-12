using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Hediff_TRAddedPart : Hediff_AddedPart
    {
        public TRHediffDef def => (TRHediffDef) base.def;
        public override string TipStringExtra
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.TipStringExtra);
                stringBuilder.AppendLine("Efficiency".Translate() + ": " + this.def.addedPartProps.partEfficiency.ToStringPercent());
                return stringBuilder.ToString();
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            //Base PostAdd() does fixed crap that we dont want, however we need the base.base.PostAdd()
            if (def.disablesNeeds != null)
            {
                pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }
            if (comps != null)
            {
                foreach (var comp in comps)
                {
                    comp.CompPostPostAdd(dinfo);
                }
            }
            if (Part == null)
            {
                Log.Error(def.defName + " has null Part. It should be set before PostAdd.", false);
                return;
            }

            pawn.health.RestorePart(Part, this, false);
            foreach (var part in Part.parts)
            {
                Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, null);
                hediff_MissingPart.IsFresh = !def.isNaturalInsertion;
                hediff_MissingPart.lastInjury = def.isNaturalInsertion ? null : HediffDefOf.SurgicalCut;
                hediff_MissingPart.Part = part;
                pawn.health.hediffSet.AddDirect(hediff_MissingPart, null, null);
            }
        }
    }
}
