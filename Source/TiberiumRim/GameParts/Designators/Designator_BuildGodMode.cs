using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Designator_BuildGodMode : Designator_Build
    {
        public TRThingDef TRThingDef => entDef as TRThingDef;

        public Designator_BuildGodMode(BuildableDef entDef) : base(entDef)
        {
        }

        [SyncWorker]
        static void SyncBuildGodMode(SyncWorker sync, ref Designator_BuildGodMode type)
        {
            if (sync.isWriting)
            {
                sync.Write(type.entDef);
            }
            else
            {
                BuildableDef entDef = sync.Read<BuildableDef>();
                type = new Designator_BuildGodMode(entDef);
            }
        }

        [SyncMethod(SyncContext.None)]
        public override void DesignateSingleCell(IntVec3 c)
        {
            Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
            GenSpawn.Spawn(thing, c, base.Map, this.placingRot, WipeMode.Vanish, false);
            FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size), base.Map);

            if (this.entDef.PlaceWorkers == null) return;
            foreach (var placeWorker in this.entDef.PlaceWorkers)
            {
                placeWorker.PostPlace(base.Map, this.entDef, c, this.placingRot);
            }
        }
    }

    /*
    public class Designator_BuildFixed : Designator_Build
    {
        private ThingDef stuffDef;
        public TRThingDef TRThingDef => entDef as TRThingDef;

        public Designator_BuildFixed(BuildableDef entdef) : base(entdef)
        {
            this.iconProportions = new Vector2(1f, 1f);
            stuffDef = (entdef?.MadeFromStuff ?? false) ? GenStuff.DefaultStuffFor(entdef) : null;
        }

        [SyncWorker(shouldConstruct = true)]
        static void SyncMyReturnType(SyncWorker sync, ref Designator_BuildFixed type)
        {
            sync.Bind(type.stuffDef, nameof(type.stuffDef));
            sync.Bind(type.entDef, nameof(type.entDef));
        }

        [SyncMethod(SyncContext.None)]
        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
                return;

            if (DebugSettings.godMode || entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef).Equals(0f))
            {
                if (this.entDef is TerrainDef)            
                    base.Map.terrainGrid.SetTerrain(c, (TerrainDef)this.entDef);         
                else
                {
                    Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
                    if(TRThingDef != null)
                        thing.SetFactionDirect(TRThingDef.devObject ? null : Faction.OfPlayer);
                    GenSpawn.Spawn(thing, c, base.Map, this.placingRot, WipeMode.Vanish, false);
                }
            }
            else
            {
                GenSpawn.WipeExistingThings(c, this.placingRot, this.entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
                GenConstruct.PlaceBlueprintForBuild_NewTemp(this.entDef, c, base.Map, this.placingRot, Faction.OfPlayer, this.stuffDef);
            }
            FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size), base.Map);
            if (this.entDef is ThingDef thingDef && thingDef.IsOrbitalTradeBeacon)
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);

            if (TutorSystem.TutorialMode)
                TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));

            if (this.entDef.PlaceWorkers == null) return;
            foreach (var placeWorker in this.entDef.PlaceWorkers)
            {
                placeWorker.PostPlace(base.Map, this.entDef, c, this.placingRot);
            }
        }
    }
    */
}
