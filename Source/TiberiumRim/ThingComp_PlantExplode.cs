using System.Collections;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ThingComp_PlantExplode : ThingComp
    {
        public CompProperties_PlantExplode properties;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.properties = (CompProperties_PlantExplode)this.props;
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (TRandom.Chance(properties.explosionChance) && dinfo.Def.isExplosive)
            {
                GenExplosion.DoExplosion(this.parent.Position, parent.Map, properties.explosiveRadius, DamageDefOf.Bomb, null, properties.damageAmountBase);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode != DestroyMode.Vanish)
            {
                if (!TRandom.Chance(properties.chanceNeverExplodeFromDamage))
                {
                    Find.CameraDriver.StartCoroutine(Explode(previousMap));
                    //aGenExplosion.DoExplosion(this.parent.Position, previousMap, properties.explosiveRadius, DamageDefOf.Bomb, null, properties.damageAmountBase);
                }
            }
            base.PostDestroy(mode, previousMap);
        }

        private IEnumerator Explode(Map map)
        {
            GenExplosion.DoExplosion(this.parent.Position, map, properties.explosiveRadius, DamageDefOf.Bomb, null, properties.damageAmountBase);
            yield return null;
        }
    }

    public class CompProperties_PlantExplode : CompProperties
    {
        public float explosiveRadius;
        public int damageAmountBase;
        public float chanceNeverExplodeFromDamage;
        public float explosionChance = 0.1f;

        public CompProperties_PlantExplode()
        {
            this.compClass = typeof(ThingComp_PlantExplode);
        }
    }
}