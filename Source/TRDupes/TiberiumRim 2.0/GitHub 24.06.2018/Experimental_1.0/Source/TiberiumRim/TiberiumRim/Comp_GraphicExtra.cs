using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_GraphicExtra : ThingComp
    {
        public Graphic FilledOverlay;
        public Graphic OverlayGraphic;

        private float fltPct = 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public ThingDef Def
        {
            get
            {
                return this.parent.def;
            }
        }

        public TiberiumContainer Container
        {
            get
            {
                if(this.parent?.TryGetComp<Comp_TNW>()?.Container != null)
                {
                    return this.parent.TryGetComp<Comp_TNW>().Container;
                }
                if((this.parent as Harvester).Container != null)
                {
                    return (this.parent as Harvester).Container;
                }
                return null;
            }
        }

        public CompPowerTrader PowerComp
        {
            get
            {
                return this.parent.GetComp<CompPowerTrader>();
            }
        }

        public CompProperties_GraphicExtra Props
        {
            get
            {
                return base.props as CompProperties_GraphicExtra;
            }
        }

        public GraphicData GraphicData
        {
            get
            {
                if(this.parent.def.graphicData != null)
                {
                    return this.parent.def.graphicData;
                }
                if((this.parent as Pawn).kindDef.lifeStages.RandomElement().bodyGraphicData != null)
                {
                    return (this.parent as Pawn).kindDef.lifeStages.RandomElement().bodyGraphicData;
                }
                return null;
            }
        }

        public bool GraphicsSet
        {
            get
            {
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                if (Props.filledOverlayGraphicPath.Length > 0)
                {
                    flag2 = FilledOverlay != null;
                }
                if (Props.overlayGraphicPath.Length > 0)
                {
                    flag3 = OverlayGraphic != null;
                }
                return flag1 && flag2 && flag3;
            }
        }

        public void SetGraphics()
        {
            SetFilledGraphic();
            SetOverlayGraphic();
        }

        public void SetFilledGraphic()
        {
            if (Props.filledOverlayGraphicPath.Length > 0)
            {
                FilledOverlay = GraphicDatabase.Get(GraphicData.graphicClass, Props.filledOverlayGraphicPath, ShaderDatabase.MoteGlow, GraphicData.drawSize, Color, Color);
            }
        }

        public void SetOverlayGraphic()
        {
            if (Props.overlayGraphicPath.Length > 0)
            {
                OverlayGraphic = GraphicDatabase.Get(GraphicData.graphicClass, Props.overlayGraphicPath, Shader, GraphicData.drawSize, GraphicData.color, GraphicData.colorTwo, GraphicData);
            }
        }

        public Shader Shader
        {
            get
            {
                ShaderType sType = GraphicData.shaderType;
                if (GraphicData.shaderType == ShaderType.None)
                {
                    sType = ShaderType.Cutout;
                }
                return ShaderDatabase.ShaderFromType(sType);
            }
        }

        public float FilledPct
        {
            get
            {
                float flt = SpecialPct;
                if (!(flt > 0))
                {
                    if (Container != null)
                    {
                        if (Container.StoredPct > 0)
                        {
                            flt = Container.StoredPct;
                        }
                    }
                }
                return flt;
            }
        }
        public float SpecialPct
        {
            get
            {
                return fltPct;
            }
            set
            {
                fltPct = value;
            }
        }

        public Color Color
        {
            get
            {
                if (Container != null)
                {
                    return Container.Color;
                }
                return Color.white;
            }
        }

        public Material FadedMaterial
        {
            get
            {
                return FilledOverlay.MatAt(parent.Rotation, null);
            }
        }

        public override void PostDraw()
        {
            if (!GraphicsSet)
            {
                SetGraphics();
            }

            if (parent != null)
            {
                if (FilledPct > 0f)
                {
                    Graphics.DrawMesh(FilledOverlay.MeshAt(parent.Rotation), parent.DrawPos + Altitudes.AltIncVect, Quaternion.identity, FadedMaterial, 0);
                }
            }
        }
    }

    public class CompProperties_GraphicExtra : CompProperties
    {
        public string filledOverlayGraphicPath = "";
        public string overlayGraphicPath = "";

        public CompProperties_GraphicExtra()
        {
            this.compClass = typeof(Comp_GraphicExtra);
        }
    }
}
