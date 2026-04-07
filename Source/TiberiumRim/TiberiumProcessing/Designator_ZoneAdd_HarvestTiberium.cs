using RimWorld;
using TR.GameParts.Networks.TiberiumNetwork;
using TR.Rendering.TextureContent;
using Verse;

namespace TR.TiberiumProcessing;

public class Designator_ZoneAdd_HarvestTiberium : Designator_ZoneAdd
{
    private readonly CompTNS_Refinery parentRefinery;

    public Designator_ZoneAdd_HarvestTiberium(CompTNS_Refinery parentRefinery)
    {
        this.parentRefinery = parentRefinery;
        zoneTypeToPlace = typeof(Zone_HarvestTiberium);
        defaultLabel = "TR_HarvestTiberiumZone".Translate();
        defaultDesc = "TR_HarvestTiberiumZoneDesc".Translate();
        icon = TiberiumContent.ZoneCreate_HarvestTiberium;
        hotKey = KeyBindingDefOf.Misc2;
    }

    public override string NewZoneLabel => "TR_HarvestTiberiumZone".Translate();

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!base.CanDesignateCell(c).Accepted) return false;

        if (c.GetTerrain(Map).passability == Traversability.Impassable) return false;
        var list = Map.thingGrid.ThingsListAt(c);
        for (var i = 0; i < list.Count; i++)
            if (!list[i].def.CanOverlapZones)
                return false;

        return true;
        //return Map.Tiberium().TiberiumProducerInfo.HasProducerAt(c, out _);
    }

    public override void Deselected()
    {
        base.Deselected();
    }

    public override Zone MakeNewZone()
    {
        var newZone = new Zone_HarvestTiberium(Find.CurrentMap.zoneManager);
        newZone.ParentRefinery = parentRefinery;
        parentRefinery.HarvestTiberiumZone = newZone;
        return newZone;
    }
}