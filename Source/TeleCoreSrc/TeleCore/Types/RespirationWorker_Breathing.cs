using TeleCore.Needs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class RespirationWorker_Breathing : RespirationWorker
{
    public HediffDef hyper;
    public float hyperSeverityPerInterval = 0.1f;
    public HediffDef hypo;

    public float hypoSeverityPerInterval = 0.1f;
    public FloatRange rangeHyper = new(0.60f, 1f);

    public FloatRange rangeHypo = new(0, 0.19f);
    public HediffDef recoverySicknessHyper;
    public HediffDef recoverySicknessHypo;

    public override void OnInterval(Pawn pawn, float needLevel, Need_Respiration need = null)
    {
        if (pawn?.health?.hediffSet == null) return;

        var hasCondition = pawn.health.hediffSet.GetFirstHediffOfDef(hypo);
        if (rangeHypo.Includes(needLevel))
        {
            var condition = hasCondition ?? pawn.health.AddHediff(hypo);
            if (condition != null)
                condition.Severity = Mathf.InverseLerp(rangeHypo.TrueMax, rangeHypo.TrueMin, needLevel);
        }
        else if (needLevel > rangeHypo.TrueMax)
        {
            if (hasCondition != null)
            {
                pawn.health.RemoveHediff(hasCondition);
                if (!pawn.health.hediffSet.HasHediff(recoverySicknessHypo))
                    pawn.health.AddHediff(recoverySicknessHypo);
            }

            var hasHighCondition = pawn.health.hediffSet.GetFirstHediffOfDef(hyper);
            if (rangeHyper.Includes(needLevel))
            {
                var highCondition = hasHighCondition ?? pawn.health.AddHediff(hyper);
                if (highCondition != null)
                    highCondition.Severity = Mathf.Lerp(rangeHyper.TrueMin, rangeHyper.TrueMax, needLevel);
            }
            else if (needLevel < rangeHyper.TrueMin && hasHighCondition != null)
            {
                pawn.health.RemoveHediff(hasHighCondition);
                if (!pawn.health.hediffSet.HasHediff(recoverySicknessHyper))
                    pawn.health.AddHediff(recoverySicknessHyper);
            }
        }
    }
}