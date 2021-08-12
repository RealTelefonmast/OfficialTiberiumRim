using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //This comp allows easy manipulation/addition of pawn elements that should be drawn
    //TODO: Define fixed offsets of specific things, eye pos, head pos, torso etc..
    //TODO: Define methods to draw additonal things on specific parts, eyes, hair..
    //TODO: Allow hediffs to draw directly
    public class CompProperties_PawnExtraDrawer : CompProperties
    {
        public CompProperties_PawnExtraDrawer()
        {
            compClass = typeof(Comp_PawnExtraDrawer);
        }
    }

    public class Comp_PawnExtraDrawer : ThingComp
    {
        //[0] = Body | [1] = Head
        public Dictionary<string, Graphic[]> drawGraphics = new Dictionary<string, Graphic[]>();
        public Dictionary<string, GraphicData[]> graphicData = new Dictionary<string, GraphicData[]>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public void RegisterParts(string id, Graphic head, Graphic body)
        {
            if (drawGraphics.ContainsKey(id)) return;
            drawGraphics.Add(id, new Graphic[]{body, head});
        }

        public void RegisterParts(string id, GraphicData head, GraphicData body)
        {
            if (drawGraphics.ContainsKey(id)) return;
            graphicData.Add(id, new GraphicData[] { body, head });
        }

        public void DeregisterParts(string id)
        {
            drawGraphics.Remove(id);
        }

        private void DrawOntoHead(Graphic head, Vector3 drawPos, Quaternion rot, bool renderBody, Rot4 bodyRot, Rot4 headRot, bool portrait, bool headStump, bool mirrored)
        {
        }

        private void DrawOntoBody()
        {

        }

        private void DrawOntoAnimal(Pawn pawn, Graphic body, Vector3 drawPos, Quaternion rot, Rot4 bodyRot)
        {
            GenDraw.DrawMeshNowOrLater(body.MeshAt(bodyRot), drawPos, rot, body.MatAt(bodyRot, pawn), false);
        }

        private bool ShouldShow(Rot4 rotation, bool mirrored)
        {
            if (rotation == Rot4.North || rotation == Rot4.South)
            {
                return true;
            }
            return mirrored ? rotation == Rot4.East : rotation == Rot4.West;
        }

        private bool ShouldMirror(Rot4 rotation, bool mirrored)
        {
            if (rotation == Rot4.North || rotation == Rot4.South)
            {
                return mirrored;
            }
            return false;
        }

        private void DrawOntoHuman(Pawn pawn, Graphic head, Graphic body, Vector3 drawPos, Quaternion rot, bool renderBody, Rot4 bodyRot, Rot4 headRot, bool portrait, bool headStump, bool mirrored)
        {
            Vector3 drawVector = drawPos;
            if (head != null && !headStump && ShouldShow(headRot, mirrored))
            {
                Vector3 a = drawPos;
                if (bodyRot != Rot4.North)
                {
                    a.y += 0.02734375f;
                    drawVector.y += 0.0234375f;
                }
                else
                {
                    a.y += 0.0234375f;
                    drawVector.y += 0.02734375f;
                }

                Vector3 b = rot * pawn.Drawer.renderer.BaseHeadOffsetAt(headRot);
                Material material = head.MatAt(headRot);
                if (material != null)
                {
                    //mirrored = right
                    Mesh mesh2 = ShouldMirror(headRot, mirrored) ? MeshPool.GridPlaneFlip(new Vector2(1.5f, 1.5f)) : MeshPool.humanlikeHeadSet.MeshAt(headRot);
                    GenDraw.DrawMeshNowOrLater(mesh2, a + b, rot, material, portrait);
                }
            }

            if (body != null && renderBody)
            {

            }
        }

        public void DrawExtraLayers(Pawn pawn, Vector3 drawPos, Quaternion rotation, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            foreach (var graphicPair in graphicData)
            {
                /*
                var graphics = graphicPair.Value;
                bool drawMirrored = graphicPair.Key.Contains("_Mirror");
                Graphic head = graphics[1];
                Graphic body = graphics[0];
                */
                var graphicData = graphicPair.Value;
                bool drawMirrored = graphicPair.Key.Contains("_Mirror");
                Graphic head = graphicData[1].Graphic;
                Graphic body = graphicData[0].Graphic;

                if (!pawn.def.race.Humanlike)
                    DrawOntoAnimal(pawn, body, drawPos, rotation, bodyFacing);
                else
                    DrawOntoHuman(pawn, head, body, drawPos, rotation, renderBody, bodyFacing, bodyFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.HeadStump), drawMirrored);
            }
        }

        /*
        public void DrawCrystal(Vector3 drawLoc, Quaternion bodyQuat, Rot4 bodyRot, Rot4 headRot, bool forPortrait, bool alpha)
        {
            bool ov = Body != null;
            PawnGraphicSet graphics = this.pawn.Drawer.renderer.graphics;
            if (pawn.RaceProps.Humanlike)
            {
                Vector3 vector = drawLoc;
                if (Head != null)
                {
                    Vector3 a = drawLoc;
                    if (bodyRot != Rot4.North)
                    {
                        a.y += 0.02734375f;
                        vector.y += 0.0234375f;
                    }
                    else
                    {
                        a.y += 0.0234375f;
                        vector.y += 0.02734375f;
                    }
                    if (graphics.headGraphic != null)
                    {
                        Vector3 b = bodyQuat * pawn.Drawer.renderer.BaseHeadOffsetAt(headRot);
                        Material material = Head.MatAt(headRot);
                        if (material != null)
                        {
                            Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headRot);
                            GenDraw.DrawMeshNowOrLater(mesh2, a + b, bodyQuat, material, forPortrait);
                        }
                    }
                }
                if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso))
                {
                    Mesh mesh = null;
                    if (pawn.RaceProps.Humanlike)
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyRot);
                    else
                        mesh = graphics.nakedGraphic.MeshAt(bodyRot);
                    Material material4 = Body.MatAt(bodyRot, null);
                    GenDraw.DrawMeshNowOrLater(mesh, vector, bodyQuat, material4, forPortrait);
                }
            }
            else
            {
                var mesh = ov ? Body.MeshAt(bodyRot) : MeshPool.plane14;
                var quat = ov ? bodyQuat : this.quat;
                var drawMat = ov ? Body.MatAt(bodyRot, pawn) : mat;
                GenDraw.DrawMeshNowOrLater(mesh, drawLoc, quat, drawMat, forPortrait);
            }
        }*/
    }
}
