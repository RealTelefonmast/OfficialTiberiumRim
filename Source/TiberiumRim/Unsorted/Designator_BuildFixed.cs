using RimWorld;
using TiberiumRim;
using UnityEngine;
using Verse;

namespace TR.Designators;

public class Designator_BuildFixed : Designator_Build
{
    private readonly ThingDef stuffDef;

    public Designator_BuildFixed(BuildableDef entdef) : base(entdef)
    {
        iconProportions = new Vector2(1f, 1f);
        stuffDef = (bool)entdef?.MadeFromStuff ? GenStuff.DefaultStuffFor(entdef) : null;
    }

    public TRThingDef TRThingDef => entDef as TRThingDef;

    public override void DesignateSingleCell(IntVec3 c)
    {
        if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(TutorTagDesignate, c)))
            return;

        if (DebugSettings.godMode || entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef).Equals(0f))
        {
            if (entDef is TerrainDef)
            {
                Map.terrainGrid.SetTerrain(c, (TerrainDef)entDef);
            }
            else
            {
                var thing = ThingMaker.MakeThing((ThingDef)entDef, stuffDef);
                if (TRThingDef != null)
                    thing.SetFactionDirect(TRThingDef.devObject ? null : Faction.OfPlayer);
                GenSpawn.Spawn(thing, c, Map, placingRot);
            }
        }
        else
        {
            GenSpawn.WipeExistingThings(c, placingRot, entDef.blueprintDef, Map, DestroyMode.Deconstruct);
            GenConstruct.PlaceBlueprintForBuild(entDef, c, Map, placingRot, Faction.OfPlayer, stuffDef);
        }

        FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size), Map);
        if (entDef is ThingDef thingDef && thingDef.IsOrbitalTradeBeacon)
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);

        if (TutorSystem.TutorialMode)
            TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, c));

        if (entDef.PlaceWorkers == null) return;
        foreach (var placeWorker in entDef.PlaceWorkers) placeWorker.PostPlace(Map, entDef, c, placingRot);
    }
}