using System.Collections;
using RimWorld;
using TR.Util;
using Verse;

namespace TR;

public class ThingComp_PlantExplode : ThingComp
{
    public CompProperties_PlantExplode properties;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        properties = (CompProperties_PlantExplode)props;
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
        if (TRUtils.Chance(properties.explosionChance) && dinfo.Def.isExplosive)
            GenExplosion.DoExplosion(parent.Position, parent.Map, properties.explosiveRadius, DamageDefOf.Bomb, null,
                properties.damageAmountBase);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        if (mode != DestroyMode.Vanish)
            if (!TRUtils.Chance(properties.chanceNeverExplodeFromDamage))
                Find.CameraDriver.StartCoroutine(Explode(previousMap));
        //aGenExplosion.DoExplosion(this.parent.Position, previousMap, properties.explosiveRadius, DamageDefOf.Bomb, null, properties.damageAmountBase);
        base.PostDestroy(mode, previousMap);
    }

    private IEnumerator Explode(Map map)
    {
        GenExplosion.DoExplosion(parent.Position, map, properties.explosiveRadius, DamageDefOf.Bomb, null,
            properties.damageAmountBase);
        yield return null;
    }
}

public class CompProperties_PlantExplode : CompProperties
{
    public float chanceNeverExplodeFromDamage;
    public int damageAmountBase;
    public float explosionChance = 0.1f;
    public float explosiveRadius;

    public CompProperties_PlantExplode()
    {
        compClass = typeof(ThingComp_PlantExplode);
    }
}