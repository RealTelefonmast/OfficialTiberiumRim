using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class ThingComp_TiberiumExplosive : ThingComp
    {
        public CompProperties_TiberiumExplosive Props
        {
            get
            {
                return base.props as CompProperties_TiberiumExplosive;
            }
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (dinfo.Def.isExplosive)
            {
                if (TRUtils.Chance(Props.explosionChance))
                {
                    GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosionRadius, DamageDefOf.Bomb, parent);
                    GenTiberium.SpawnSpore(parent.OccupiedRect(), Props.radius, parent.Map, Props.tiberiumTypes.RandomElement(), null, Props.sporeAmount, true);
                    parent.Destroy(DestroyMode.KillFinalize);
                }
            }
        }
    }

    public class CompProperties_TiberiumExplosive : CompProperties
    {
        public float explosionChance = 0.5f;
        public float explosionRadius = 7f;
        public int sporeAmount = 0;
        public int radius = 5;
        public List<TiberiumCrystalDef> tiberiumTypes;

        public CompProperties_TiberiumExplosive()
        {
            compClass = typeof(ThingComp_TiberiumExplosive);
        }
    }
}
