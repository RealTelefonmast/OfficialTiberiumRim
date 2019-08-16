﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class HediffRelativeDef : HediffDef
    {
        public List<PawnCapacityModifier> relativeCapMods = new List<PawnCapacityModifier>();
        public float relativePartEfficiency = 0;
        public float relativePainFactor = 0;
        public int capacityInterval = 750;
    }

    public class Hediff_Relative : HediffWithComps
    {
        public new HediffRelativeDef def;

        public override void PostMake()
        {
            base.PostMake();
            def = (HediffRelativeDef)base.def;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(def.capacityInterval))
            {
                pawn.health.Notify_HediffChanged(this);
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                var cur = base.CurStage;
                var s = Severity;
                return new HediffStage
                {
                    becomeVisible              = cur.becomeVisible,
                    deathMtbDays               = cur.deathMtbDays,
                    destroyPart                = cur.destroyPart,
                    forgetMemoryThoughtMtbDays = cur.forgetMemoryThoughtMtbDays,
                    hediffGivers               = cur.hediffGivers,
                    hungerRateFactor           = cur.hungerRateFactor,
                    hungerRateFactorOffset     = cur.hungerRateFactorOffset,
                    label                           = cur.label,
                    lifeThreatening                 = cur.lifeThreatening,
                    makeImmuneTo                    = cur.makeImmuneTo,
                    mentalBreakMtbDays              = cur.mentalBreakMtbDays,
                    mentalStateGivers               = cur.mentalStateGivers,
                    minSeverity                     = cur.minSeverity,
                    opinionOfOthersFactor           = cur.opinionOfOthersFactor,
                    painOffset                      = cur.painOffset,
                    partIgnoreMissingHP             = cur.partIgnoreMissingHP,
                    pctConditionalThoughtsNullified = cur.pctConditionalThoughtsNullified,
                    restFallFactor                  = cur.restFallFactor,
                    restFallFactorOffset            = cur.restFallFactorOffset,
                    socialFightChanceFactor         = cur.socialFightChanceFactor,
                    statOffsets                     = cur.statOffsets,
                    tale                            = cur.tale,
                    untranslatedLabel               = cur.untranslatedLabel,
                    vomitMtbDays                    = cur.vomitMtbDays,
                    //Relative
                    painFactor = cur.painFactor * RelativeModifier,
                    partEfficiencyOffset = def.relativePartEfficiency * RelativeModifier,
                    capMods = RelativeCapMods                  
                };
            }
        }

        public float RelativeModifier => Severity;

        public List<PawnCapacityModifier> RelativeCapMods
        {
            get
            {
                List<PawnCapacityModifier> mods = new List<PawnCapacityModifier>();
                foreach (var cap in def.relativeCapMods)
                {
                    mods.Add(new PawnCapacityModifier()
                    {
                        capacity = cap.capacity,
                        offset = cap.offset * Severity,
                        postFactor = cap.postFactor,
                        setMax = cap.setMax,
                    });
                }
                return mods;
            }
        }
    }
}
