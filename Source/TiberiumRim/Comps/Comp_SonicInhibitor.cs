using RimWorld;
using TiberiumRim;
using Verse;

namespace TR.GDI;

public class Comp_SonicInhibitor : ThingComp
{
    public CompProperties_SonicInhibitor Props => (CompProperties_SonicInhibitor)props;

    public override void CompTickRare()
    {
        FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.PsycastAreaEffect, Props.radius);
        FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.PsycastAreaEffect, Props.radius * 0.35f);
        foreach (var intVec3 in GenRadial.RadialCellsAround(parent.Position, Props.radius, true))
        {
            var tib = intVec3.GetTiberium(parent.Map);
            tib?.TakeDamage(new DamageInfo(TRDamageDefOf.TRSonic, TRUtils.Range(Props.damageRange)));
        }
    }

    private void StartEmission()
    {
    }
}

public class CompProperties_SonicInhibitor : CompProperties
{
    public FloatRange damageRange = new(2, 10);
    public float radius = 10;

    public CompProperties_SonicInhibitor()
    {
        compClass = typeof(Comp_SonicInhibitor);
    }
}