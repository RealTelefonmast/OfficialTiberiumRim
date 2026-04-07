using RimWorld;
using TeleCore.Defs.Extensions;
using TeleCore.Logging;
using TeleCore.Static;
using Verse;

namespace TeleCore.RWExtended.Hediffs;

public class HediffComp_ExplodeOnPartDestroyed : HediffComp
{
    private bool hasExploded;

    public HediffCompProperties_ExplodeOnPartDestroyed Props => (HediffCompProperties_ExplodeOnPartDestroyed)props;

    public override string? CompLabelInBracketsExtra => hasExploded
        ? Props.labelWhenExploded ?? Translations.Hediffs.ExplodedHediffRuptured.Translate()
        : null;

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref hasExploded, "ruptured");
    }

    public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        if (Pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
            Rupture(Props.chanceToExplodeOnPartDestroyed);
        else
            Rupture(Props.chanceToExplodeOnHit);

        base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
    }

    public override void Notify_PawnKilled()
    {
        Rupture(0.5f, false);
    }

    private void Rupture(float intensity, bool dealDamage = true)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (!Pawn.Spawned)
        {
            Pawn.TakeDamage(new DamageInfo(Props.explosionProps.damageDef, parent.Part.def.hitPoints, 1));
            return;
        }

        if (Props.explosionProps != null)
        {
            Props.explosionProps.DoExplosion(Pawn.Position, Pawn.Map, Pawn);
            if (dealDamage)
                Pawn.TakeDamage(new DamageInfo(Props.explosionProps.damageDef, parent.Part.def.hitPoints, 1));
        }
        else
        {
            TLog.Warning($"Tried to explode Part {parent.Part} but had no explosionProps set for {parent.def}.");
        }

        if (Props.destroyBody) Pawn.Destroy();

        if (Props.destroyGear)
            Pawn.equipment?.equipment.ClearAndDestroyContents();
        else if (Props.damageToGear.Average > 0)
            if (Pawn.equipment != null)
                foreach (var equipment in Pawn.equipment.AllEquipmentListForReading)
                    equipment.TakeDamage(new DamageInfo(DamageDefOf.Bomb, Props.damageToGear.RandomInRange));
    }
}

//TODO: Document
public class HediffCompProperties_ExplodeOnPartDestroyed : HediffCompProperties
{
    public float chanceToExplodeOnHit = 0.25f;
    public float chanceToExplodeOnPartDestroyed = 1f;
    public IntRange damageToGear = new(0, 0);
    public bool destroyBody;
    public bool destroyGear;
    public ExplosionProperties explosionProps;
    public string labelWhenExploded;

    public HediffCompProperties_ExplodeOnPartDestroyed()
    {
        compClass = typeof(HediffComp_ExplodeOnPartDestroyed);
    }
}