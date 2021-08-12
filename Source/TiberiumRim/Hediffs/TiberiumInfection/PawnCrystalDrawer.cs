using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PawnCrystalDrawer
    {
        protected Pawn pawn;
        private CrystalOverlay crystal;

        public PawnCrystalDrawer(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public bool HasImmunity => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumImmunity);
        public bool HasTrait => (pawn.story != null && pawn.story.traits.allTraits.Any(t => t.def.defName == "TiberiumTrait"));

        public void RenderOverlay(Pawn pawn, Vector3 drawLoc, Rot4 headRot, Quaternion quat, bool forPortrait)
        {

            bool immunity = HasImmunity;
            bool hasTrait = HasTrait;
            if (!immunity && !hasTrait)
                return;

            crystal ??= new CrystalOverlay(pawn, immunity);
            crystal.DrawCrystal(drawLoc, quat, pawn.Rotation, headRot, forPortrait, hasTrait);
        }
    }

    public class CrystalOverlay
    {
        private Pawn pawn;
        private Material mat;
        private Graphic Head;
        private Graphic Body;
        private Quaternion quat;
        private static readonly Vector2 crystalSpan = new Vector2(0.5f, 0.7f);

        public CrystalOverlay(Pawn pawn, bool HasImmunity)
        {
            this.pawn = pawn;
            if (pawn.RaceProps.Humanlike)
            {
                TRUtils.GetTiberiumMutant(pawn, out Head, out Body);
            }

            //Body for animals
            if (Head == null)
            {
                Vector2 drawSize = pawn.Drawer.renderer.graphics.nakedGraphic.drawSize;
                string path = pawn.Drawer.renderer.graphics.nakedGraphic.path;
                if (ContentFinder<Texture2D>.Get(path + "_TibBody", false) != null) Body = GraphicDatabase.Get(typeof(Graphic_Multi), path + "_TibBody", ShaderDatabase.Cutout, drawSize, Color.white, Color.white);
                else if (ContentFinder<Texture2D>.Get("Pawns/TiberiumOverlays" + pawn.def.defName + "/" + pawn.def.defName + "_TibBody", false) != null)
                    Body = GraphicDatabase.Get(typeof(Graphic_Multi), "Pawns/TiberiumOverlays" + pawn.def.defName + "/" + pawn.def.defName + "_TibBody", ShaderDatabase.Cutout, drawSize, Color.white, Color.white);
                else
                    Body = null;
                
            }
            if (Body == null)
            {
                this.mat = MaterialPool.MatFrom("Pawns/TiberiumMutant/Bodies/Fat_north", ShaderDatabase.MoteGlow, Color.white);
                float num = pawn.GetHashCode();
                float rand = Mathf.Clamp(num / 24f, 0, 360);
                this.quat = Quaternion.AngleAxis(rand, Vector3.up);
            }
        }

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
        }
    }
}
