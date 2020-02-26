using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class StatWorker_Tiberium : StatWorker
    {

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            
            base.FinalizeValue(req, ref val, applyPostProcess);
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing is Pawn;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            StringBuilder stringBuilder = new StringBuilder();
            float baseValueFor = this.GetBaseValueFor(req);
            if (baseValueFor != 0f)
            {
                stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + this.stat.ValueToString(baseValueFor, numberSense));
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine(GearExplenation(req));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        protected virtual string GearExplenation(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null) return "";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
            if (pawn.apparel != null)
            {
                var count = pawn.apparel.WornApparelCount;
                for (int l = 0; l < count; l++)
                {
                    Apparel gear = pawn.apparel.WornApparel[l];
                    stringBuilder.AppendLine(TextFromGear(gear, count));
                }
            }
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                stringBuilder.AppendLine(TextFromGear(pawn.equipment.Primary));
            }
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        private string TextFromGear(Thing gear, int gearCount = 1)
        {
            float f = OffsetFromGear(gear, gearCount);
            return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
        }

        protected virtual float OffsetFromGear(Thing gear, int gearCount)
        {
            return gear.def.equippedStatOffsets.GetStatOffsetFromList(this.stat);
        }

        protected virtual float PawnCapacityOffset(Pawn pawn)
        {
            return 0;
        }
    }

    public class StatWorker_TiberiumInfResistance : StatWorker_Tiberium
    {

        protected override float OffsetFromGear(Thing gear, int gearCount)
        {
            float baseVal = base.OffsetFromGear(gear, gearCount);
            baseVal += 1f - Mathf.Clamp01(gear.GetStatValue(StatDefOf.ToxicSensitivity) / gearCount);
            baseVal += 1f - Mathf.Clamp01(gear.GetStatValue(StatDefOf.ArmorRating_Sharp) / gearCount);
            return baseVal;
        }

        private static float InfectionChance(Pawn pawn, bool isGas)
        {
            //TODO: Adjust for tiberium resistance
            float num = TRUtils.Range(0.25f, 1f);
            if (isGas)
            {
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
            }
            else
            {
                num *= 1f - pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
                if (pawn.apparel != null)
                {
                    float count = pawn.apparel.WornApparelCount;
                    foreach (Apparel apparel in pawn.apparel.WornApparel)
                    {
                        num *= 1f - Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicSensitivity) / count);
                        num *= 1f - Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) / count);
                    }
                }
            }
            return num;
        }
    }

    public class StatWorker_TiberiumGasResistance : StatWorker_Tiberium
    {

    }

    public class StatWorker_TiberiumRadResistance : StatWorker_Tiberium
    {

    }

    public class StatWorker_TiberiumDamageResistance : StatWorker_Tiberium
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing.def.useHitPoints;
        }
    }
}
