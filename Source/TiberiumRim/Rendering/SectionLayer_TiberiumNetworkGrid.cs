using RimWorld;
using TR.Designators;
using Verse;

namespace TiberiumRim;

public class SectionLayer_TiberiumNetworkGrid : SectionLayer_Things
{
    public SectionLayer_TiberiumNetworkGrid(Section section) : base(section)
    {
        requireAddToMapMesh = false;
        relevantChangeTypes = MapMeshFlag.Buildings;
    }

    public override void DrawLayer()
    {
        var designator = Find.DesignatorManager.SelectedDesignator as Designator_Build;
        if (designator != null &&
            ((designator.PlacingDef as ThingDef)?.comps.Any(c => c is CompProperties_TNW) ?? false)) base.DrawLayer();
        Designator_RemoveTiberiumPipe designator2 =
            Find.DesignatorManager.SelectedDesignator as Designator_RemoveTiberiumPipe;
        if (designator2 != null) base.DrawLayer();
    }

    protected override void TakePrintFrom(Thing t)
    {
        if (t is TiberiumNetworkBuilding) (t as TiberiumNetworkBuilding).PrintForGrid(this);
    }
}