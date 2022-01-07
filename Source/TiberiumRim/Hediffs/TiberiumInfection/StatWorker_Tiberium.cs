using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum StatPartMode
    {
        Multiply,
        Offset
    }

    public abstract class StatPart_Tiberium : StatPart
    {
        private StatPartMode mode;

        public virtual bool IsDisabledFor(Thing thing)
        {
            throw new NotImplementedException();
        }

        public virtual float Value(StatRequest req)
        {
            throw new NotImplementedException();
        }

        public sealed override void TransformValue(StatRequest req, ref float val)
        {
            if (IsDisabledFor(req.Thing)) return;
            switch (mode)
            {
                case StatPartMode.Multiply:
                    val *= Value(req);
                    break;
                case StatPartMode.Offset:
                    val += Value(req);
                    break;
            }
        }

        public string ValueString(StatRequest req)
        {
            char symbol = mode == StatPartMode.Multiply ? 'x' : '+';
            return $"{symbol}{Value(req).ToStringPercent()}";
        }

        public virtual string Explanation(StatRequest req)
        {
            throw new NotImplementedException();
        }

        public sealed override string ExplanationPart(StatRequest req)
        {
            if(IsDisabledFor(req.Thing)) return String.Empty;
            return Explanation(req);
        }
    }

    public class StatPart_IsMechanoid : StatPart_Tiberium
    {
        public override bool IsDisabledFor(Thing thing)
        {
            return thing is Pawn p && !p.IsMechanoid();
        }

        public override float Value(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null && pawn.IsMechanoid())
                return 1f;
            return 0f;
        }

        public override string Explanation(StatRequest req)
        {
            return $"{"TR_StatPartIsMechanoid".Translate()}: {ValueString(req)}";
        }
    }

    public class StatWorker_Tiberium : StatWorker
    {
        public override bool IsDisabledFor(Thing thing)
        {
            if (thing is MechanicalPawn) return false;
            return base.IsDisabledFor(thing);
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing.def.category == ThingCategory.Pawn;
        }

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            base.FinalizeValue(req, ref val, applyPostProcess);
        }

        private static float InfectionChance(Pawn pawn, bool isGas)
        {
            float num = 1f;
            if (isGas)
            {
                float infFactor = 1f;
                infFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance);

                if (!pawn.CanBeInfected(true, out float gasFac)) return 0;

                num = gasFac;
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
            }
            else
            {
                float infFactor = 1f;
                infFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);

                if (!pawn.CanBeInfected(false, out float infFact)) return 0;

                num = infFact;
                if (pawn.apparel != null)
                {
                    float tox = 0, sharp = 0;
                    foreach (Apparel apparel in pawn.apparel.WornApparel)
                    {
                        tox += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicSensitivity));
                        sharp += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp));
                    }

                    num *= tox;
                    num *= 1 - Mathf.Clamp01(sharp);
                    //Log.Message("Infection Chance Apparel: Tox: " + tox + " | Sharp: " + sharp);
                }
            }
            return num;
        }
    }

    public class StatWorker_TiberiumInfResistance : StatWorker_Tiberium
    {
    }

    public class StatWorker_TiberiumGasResistance : StatWorker_Tiberium
    {

    }

    public class StatWorker_TiberiumRadResistance : StatWorker_Tiberium
    {
    }

    public class StatWorker_TiberiumCorrosionResistance : StatWorker_Tiberium
    {
    }

    public class StatWorker_TiberiumDamageResistance : StatWorker_Tiberium
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing is not Pawn;
        }
    }
}
