using TR.AI;
using Verse;

namespace TR.Comps;

public class Comp_WanderProps : ThingComp
{
    private int radialCells;

    public CompProperties_WanderProps Props => props as CompProperties_WanderProps;

    public IPawnWithParent IPawn => parent as IPawnWithParent;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        radialCells = GenRadial.NumCellsInRadius(Props.radius);
    }

    public IntVec3 GetRandomCell()
    {
        if (Props.useRadius)
            return IPawn.Parent.Position + GenRadial.RadialPattern[Rand.Range(0, radialCells)];
        ;
        return
            IPawn.Field.RandomElement();
    }
}

public class CompProperties_WanderProps : CompProperties
{
    public float radius = 5f;
    public bool useRadius = true;

    public CompProperties_WanderProps()
    {
        compClass = typeof(Comp_WanderProps);
    }
}