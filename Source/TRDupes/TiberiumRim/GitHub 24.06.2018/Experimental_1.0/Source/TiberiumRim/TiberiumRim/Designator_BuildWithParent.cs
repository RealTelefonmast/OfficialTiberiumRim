using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using UnityEngine;

namespace TiberiumRim
{
    public class Designator_BuildWithParent : Designator_Build
    {
        private new BuildableDef entDef;

        private ThingDef stuffDef;

        private Building_TNC parent;

        public Designator_BuildWithParent(BuildableDef entDef, Building_TNC parent) : base(entDef)
        {
            this.entDef = entDef;
            this.parent = parent;
        }

        public new void SetStuffDef(ThingDef stuffDef)
        {
            this.stuffDef = stuffDef;
        }

        /*
        public override void DrawMouseAttachments()
        {
            int count = (int)parent.Position.DistanceTo(UI.MouseCell()) * 5;
            ThingCountClass counter = entDef.costList.Find((ThingCountClass x) => x.thingDef == ThingDefOf.Steel);
            if(counter != null)
            {
                counter.count = count;
            }
            else
            {
                entDef.costList.Add(new ThingCountClass(ThingDefOf.Steel, count));
            }
            base.DrawMouseAttachments();
        }
        */

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
            {
                return;
            }
            if (DebugSettings.godMode || this.entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, this.stuffDef) == 0f)
            {
                if (this.entDef is TerrainDef)
                {
                    base.Map.terrainGrid.SetTerrain(c, (TerrainDef)this.entDef);
                }
                else
                {
                    Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
                    thing.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(thing, c, base.Map, this.placingRot, false);
                    CellRect buildingRect = thing.OccupiedRect();
                    this.parent.Positions.Add(buildingRect);
                }
            }
            else
            {               
                TRUtils.MakeNewBluePrint((ThingDef)entDef, false, null);
                TRUtils.MakeNewFrame((ThingDef)entDef);
                GenSpawn.WipeExistingThings(c, this.placingRot, this.entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
                Blueprint_Build builder = GenConstruct.PlaceBlueprintForBuild(this.entDef, c, base.Map, this.placingRot, Faction.OfPlayer, this.stuffDef);
                CellRect buildingRect = builder.OccupiedRect();
                this.parent.Positions.Add(buildingRect);
            }
            MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size), base.Map);
            if (this.entDef is ThingDef thingDef && thingDef.IsOrbitalTradeBeacon)
            {
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
            }
            if (TutorSystem.TutorialMode)
            {
                TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));
            }
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
