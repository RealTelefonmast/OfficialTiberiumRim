using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Designator_BuildFixed : Designator_Build
    {
        private ThingDef stuffDef;

        public Designator_BuildFixed(BuildableDef entdef) : base(entdef)
        {
            this.iconProportions = new Vector2(1f, 1f);
            stuffDef = (bool)entdef?.MadeFromStuff ? GenStuff.DefaultStuffFor(entdef) : null;
        }

        public TRThingDef TRThingDef => entDef as TRThingDef;

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
                return;

            if (DebugSettings.godMode || this.entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef) == 0f)
            {
                if (this.entDef is TerrainDef)            
                    base.Map.terrainGrid.SetTerrain(c, (TerrainDef)this.entDef);         
                else
                {
                    Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
                    thing.SetFactionDirect(TRThingDef.devObject ? null : Faction.OfPlayer);
                    GenSpawn.Spawn(thing, c, base.Map, this.placingRot, WipeMode.Vanish, false);
                }
            }
            else
            {
                GenSpawn.WipeExistingThings(c, this.placingRot, this.entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
                GenConstruct.PlaceBlueprintForBuild(this.entDef, c, base.Map, this.placingRot, Faction.OfPlayer, this.stuffDef);
            }
            MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size), base.Map);
            ThingDef thingDef = this.entDef as ThingDef;
            if (thingDef != null && thingDef.IsOrbitalTradeBeacon)
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);

            if (TutorSystem.TutorialMode)
                TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));

            if (this.entDef.PlaceWorkers != null)
            {
                for (int i = 0; i < this.entDef.PlaceWorkers.Count; i++)
                {
                    this.entDef.PlaceWorkers[i].PostPlace(base.Map, this.entDef, c, this.placingRot);
                }
            }
        }
    }
}
