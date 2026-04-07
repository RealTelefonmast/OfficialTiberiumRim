using Verse;

namespace TeleCore.Atmosphere.Oxygen;

public class Hediff_Hypoxia : Hediff
{
    private int ticksSinceHypoxic;

    public override void Tick()
    {
        base.Tick();

        if (!(Severity >= 0.5f)) return;
        ticksSinceHypoxic++;
        var seconds = ticksSinceHypoxic.TicksToSeconds();
        if (seconds > 10f && !pawn.health.hediffSet.HasHediff(NMODefOf.CerebralHypoxia))
            if (Rand.Chance(Rand.ValueSeeded(pawn.thingIDNumber ^ 2551674) * 0.25f))
            {
                //Brain Damage
                var brain = pawn.health.hediffSet.GetBrain();
                if (brain != null) pawn.health.AddHediff(NMODefOf.CerebralHypoxia, brain);
            }
    }

    public override bool CauseDeathNow()
    {
        return ticksSinceHypoxic > 20f * GenTicks.TicksPerRealSecond;
    }
}