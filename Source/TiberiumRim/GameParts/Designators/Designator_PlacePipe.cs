using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Designator_PlaceThing : Designator_Build
    {
        public Designator_PlaceThing(BuildableDef def) : base(def)
        {
            icon = PlacingDef.uiIcon;
            this.entDef = def;
        }

        public override BuildableDef PlacingDef => entDef;
        public override string Label => PlacingDef.label;
        public override string Desc => PlacingDef.description;
        public override bool Visible => true;
        public override int DraggableDimensions => 1;
        public override bool DragDrawMeasurements => true;
        public override float PanelReadoutTitleExtraRightMargin => 20f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }

        /*
        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return GenConstruct.CanPlaceBlueprintAt(PlacingDef, loc, placingRot, Map, DebugSettings.godMode);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (DebugSettings.godMode)
            {
                TNW_Pipe pipe = ThingMaker.MakeThing((ThingDef)PlacingDef) as TNW_Pipe;
                pipe.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(pipe, c, base.Map, this.placingRot, WipeMode.Vanish, false);
            }
            else
            {
                GenSpawn.WipeExistingThings(c, this.placingRot, PlacingDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
                Blueprint_Build blueprint = (Blueprint_Build)ThingMaker.MakeThing(PlacingDef.blueprintDef, null);
                blueprint.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(blueprint, c, Map);
            }
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }
        */
    }
}
