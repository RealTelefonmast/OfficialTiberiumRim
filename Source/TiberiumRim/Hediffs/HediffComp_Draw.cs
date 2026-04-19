using Verse;

namespace TR.Comps;

public class HediffComp_Draw : HediffComp
{
    public bool MirrorImage => parent.Part.customLabel.Contains("right");

    public TeleCore.Rendering.Comp_PawnExtraDrawer Drawer => Pawn.GetComp<TeleCore.Rendering.Comp_PawnExtraDrawer>();

    public HediffCompProperties_Draw Props => (HediffCompProperties_Draw)props;

    public string Identifier => MirrorImage ? Props.identifier + "_Mirror" : Props.identifier;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        Drawer.RegisterParts(Identifier, Props.headGraphic, Props.bodyGraphic);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        Drawer.DeregisterParts(Identifier);
    }
}

public class HediffCompProperties_Draw : HediffCompProperties
{
    public GraphicData bodyGraphic;
    public GraphicData headGraphic;
    public string identifier;

    public HediffCompProperties_Draw()
    {
        compClass = typeof(HediffComp_Draw);
    }
}