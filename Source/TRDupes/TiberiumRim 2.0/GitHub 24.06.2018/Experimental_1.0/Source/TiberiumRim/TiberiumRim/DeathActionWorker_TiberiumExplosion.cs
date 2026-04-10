using System;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class DeathActionWorker_TiberiumExplosion : DeathActionWorker
    {
        public override RulePackDef DeathRules
        {
            get
            {
                return RulePackDefOf.Transition_DiedExplosive;
            }
        }

        public override void PawnDied(Corpse corpse)
        {
            float radius;
            if (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0)
            {
                radius = 1.9f;
            }
            else if (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 1)
            {
                radius = 2.9f;
            }
            else
            {
                radius = 4.9f;
            }
            GenExplosion.DoExplosion(corpse.Position, corpse.Map, radius, DamageDefOf.Flame, corpse.InnerPawn, -1, null, null, null, TiberiumDefOf.TiberiumBlue, 0.15f, 1, false, null, 0f, 1, 0f, false);
        }
    }
}
