using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class DamageWorker_SoftExplosion : DamageWorker
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
			if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }

            FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash, explosion.radius * 6f);

            if (explosion.Map == Find.CurrentMap)
            {
                float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
            }
            ExplosionVisualEffectCenter(explosion);
		}
    }
}
