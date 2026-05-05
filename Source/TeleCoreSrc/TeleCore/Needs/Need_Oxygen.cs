using RimWorld;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Hediffs;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Needs;

public enum OxygenCategory : byte
{
    Saturated,
    Low,
    Hypoxia
}

//Basis:
// We assume a human breathes 1L of pure oxygen per minute
// This implies 60L per hour
// One unit of oxygen is 10L
// The standard breathable oxygen level is 21% of the atmosphere
// Hypoxia begins at 19.5% -> a drop of 1.5% is enough to start being dangerous
// The player should

public class Need_Oxygen : Need
{
    private readonly Comp_PawnAtmosphereTracker _atmosTracker;

    public Need_Oxygen(Pawn pawn) : base(pawn)
    {
        _atmosTracker = Comp_PawnAtmosphereTracker.CompFor(pawn);
        BreathingProps = pawn.kindDef.GetModExtension<BreathingExtension>();
    }

    public BreathingExtension BreathingProps { get; }

    public float BreathingLevelRequired => BreathingProps?.OxygenLevelPercentageWantBreathe ?? 0.75f;
    public float PercentageThreshUrgentlyOxygenDeprived => BreathingLevelRequired * 0.4f;

    //Consumption Rate
    public float RatePerHour => 0.125F; // ~ 1.5 / 15h |
    public float RatePerMinute => 0.001953125F; //Approximation of 0.125F / 60 in binary

    /*
    private float HypoxiaFactorBase
    {
        get
        {
            float baseFactor = 0f;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Breathing)) return 0f;

            var breathing = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
            var bloodPumping = pawn.health.capacities.GetLevel(PawnCapacityDefOf.BloodPumping);
            var consciousness = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);

            baseFactor += 1 - (breathing * bloodPumping * consciousness);

            // Incorporate health conditions
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.Hypothermia))
            {
                baseFactor *= 1.25f;
            }

            if (pawn.health.hediffSet.HasHediff(HediffDefOf.Asthma))
            {
                baseFactor *= 2;
            }

            return 1 + baseFactor;
        }
    }

    private float HypoxiaSeverityPerInterval
    {
        get
        {
            var hypoxiaFactor = HypoxiaFactorBase;
            if (hypoxiaFactor <= 0) return 0;

            var levelFactor = 1f - CurLevel;
            return hypoxiaFactor * levelFactor * (HypoxiaSeverityFactor * Mathf.Lerp(0.8f, 1.2f, Rand.ValueSeeded(pawn.thingIDNumber ^ 2551674)));
        }
    }
    */

    //States
    public bool Suffocating => CurCategory == OxygenCategory.Hypoxia;
    public override bool IsFrozen => base.IsFrozen || pawn.Deathresting;

    //TODO: Add a way to track whether room is getting filled or not
    public override int GUIChangeArrow { get; }

    public OxygenCategory CurCategory
    {
        get
        {
            if (CurLevelPercentage <= 0f) return OxygenCategory.Hypoxia;
            if (CurLevelPercentage < PercentageThreshUrgentlyOxygenDeprived) return OxygenCategory.Low;
            return OxygenCategory.Saturated;
        }
    }

    public override void NeedInterval()
    {
        if (!pawn.Spawned)
            //Not Spawned...
            return;

        if (_atmosTracker != null)
        {
            CurLevel = _atmosTracker.RoomComp.Volume.StoredPercentOf(NMODefOf.Atmosphere_Oxygen);

            if (!_atmosTracker.IsOutside)
                _atmosTracker.RoomComp.Volume.TryRemove(NMODefOf.Atmosphere_Oxygen, 2);
        }

        if (!IsFrozen || pawn.Deathresting)
        {
            var hasHypoxia = pawn.health.hediffSet.GetFirstHediff<Hediff_Hypoxia>();
            if (Suffocating)
            {
                var hypoxia = hasHypoxia ?? (Hediff_Hypoxia)pawn.health.AddHediff(NMODefOf.Hypoxia);
                hypoxia.Severity += 0.1f;
            }
            else if (hasHypoxia != null)
            {
                pawn.health.RemoveHediff(hasHypoxia);
                if (!pawn.health.hediffSet.HasHediff(NMODefOf.HypoxiaSickness))
                    pawn.health.AddHediff(NMODefOf.HypoxiaSickness);
            }
        }
    }
}