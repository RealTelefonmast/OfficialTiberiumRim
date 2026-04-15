using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Designator_PlacePipe : Designator_Place
    {
        private TNW_TNC parent;
        private Graphic_LinkedTNW graphic = new Graphic_LinkedTNW();

        private NetworkMode NetworkMode => parent.Network.NetworkMode;

        public Designator_PlacePipe(TNW_TNC parent)
        {
            this.parent = parent;
            this.icon = PlacingDef.uiIcon;

        }

        public override BuildableDef PlacingDef => TiberiumDefOf.TiberiumPipe;
        public override string Label => PlacingDef.label;
        public override string Desc => "PlacePipe_TR".Translate();
        public override bool Visible => true;
        public override int DraggableDimensions => 1;
        public override bool DragDrawMeasurements => true;
        public override float PanelReadoutTitleExtraRightMargin => 20f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
            Rect topL = new Rect(topLeft.x + 5f, topLeft.y, 50f, 30f);

            Text.Font = GameFont.Medium;
            GUI.color = Color.cyan;
            Rect topR = new Rect(topLeft.x + maxWidth - 5f, topLeft.y + maxWidth - 15f, 20f, 20f);
            Widgets.Label(topL, parent.Network.GreekLetter);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            //GUI.TextArea(topL, parent.Network.NetworkMode.ToString());
            return result;
        }

        protected override void DrawGhost(Color ghostCol)
        {
            PlacingDef.graphic.DrawFromDef(UI.MouseMapPosition(), placingRot, (ThingDef)PlacingDef);
        }

        public override void DrawMouseAttachments()
        {
            /*
            Vector2 mousePos = Event.current.mousePosition;
            IntVec3 vec = Current.Camera.
            Vector3 mousePosV3 = new Vector3(mousePos.x, 0, mousePos.y);
            Rect mouseRect = new Rect();
            graphic.DrawWorker(Event.current.mousePosition,)
            base.DrawMouseAttachments();
            */
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return GenConstruct.CanPlaceBlueprintAt(PlacingDef, loc, placingRot, Map, DebugSettings.godMode);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (DebugSettings.godMode)
            {
                TNW_Pipe pipe = ThingMaker.MakeThing((ThingDef)PlacingDef) as TNW_Pipe;
                pipe.predefinedMode = NetworkMode;
                pipe.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(pipe, c, base.Map, this.placingRot, WipeMode.Vanish, false);

            }
            else
            {

            }
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }
    }
}
