using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class PlantComp_Explode : ThingComp
    {
        public Map map;
        public CompProperties_PlantExplode properties;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.map = this.parent.Map;
            this.properties = (CompProperties_PlantExplode)this.props;
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode != DestroyMode.Vanish)
            {
                if (Rand.Chance(properties.chanceNeverExplodeFromDamage))
                {
                    GenExplosion.DoExplosion(this.parent.Position, map, properties.explosiveRadius, DamageDefOf.Bomb, null, properties.damageAmountBase);
                }
            }
            base.PostDestroy(mode, previousMap);
        }
    }

    public class CompProperties_PlantExplode : CompProperties
    {
        public float explosiveRadius;
        public int damageAmountBase;
        public float chanceNeverExplodeFromDamage;

        public CompProperties_PlantExplode()
        {
            this.compClass = typeof(PlantComp_Explode);
        }
    }
}