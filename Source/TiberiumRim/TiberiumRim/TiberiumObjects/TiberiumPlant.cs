using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumPlant : Plant, IFXObject
    {
        public new TRThingDef def => (TRThingDef)base.def;

        public TiberiumGarden parentGarden;

        private Graphic graphicInt2;
        private static Color32[] colors = new Color32[4];

		public WorldComponent_TR TiberiumRimComp => Find.World.GetComponent<WorldComponent_TR>();
        public MapComponent_Tiberium TiberiumMapComp => Map.GetComponent<MapComponent_Tiberium>();

		//FX STUFF
        public ExtendedGraphicData ExtraData => (base.def as FXThingDef).extraData;
        public CompFX FXComp => this.GetComp<CompFX>();

        public virtual Vector3[] DrawPositions => new Vector3[1] { base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[1] { Color.white };
        public virtual float[] OpacityFloats => new float[1] { 1f };
        public virtual float?[] RotationOverrides => new float?[1] { null };
        public virtual bool[] DrawBools => new bool[1] { true };
        public virtual Action<FXGraphic>[] Actions => null;

        public virtual Vector2? TextureOffset => null;
        public virtual Vector2? TextureScale => null;
        public virtual bool ShouldDoEffecters => true;

        public virtual CompPower ForcedPowerComp => null;


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumMapComp.RegisterTiberiumPlant(this);
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {

            TiberiumMapComp.DeregisterTiberiumPlant(this);
            base.DeSpawn(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            Log.Message(this + " being damaged by " + dinfo.Def + " with " + totalDamageDealt);
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override bool BlightableNow => false;
        public override bool IngestibleNow => false;

        public override float CurrentDyingDamagePerTick
        {
            get
            {
                return base.CurrentDyingDamagePerTick;
            }
        }


        public override void Draw()
        {
            base.Draw();
        }

        public Graphic OverlayGraphic
        {
            get
            {
                if (graphicInt2 == null)
                {
                    if (def.graphicData2 == null)
                    {
                        return null;
                    }
                    graphicInt2 = this.def.graphicData2.GraphicColoredFor(this);
                    return graphicInt2;
					if (Graphic is Graphic_Random random)
                    {
                        var path = def.graphicData2.texPath;
						//Log.Message(random  + " | " + random?.SubGraphicFor(this) + " | " + random?.SubGraphicFor(this)?.data + " | ");
                        var graphic = random.SubGraphicFor(this);
                        var suffix = graphic.path.Split('/').Last();
						Log.Message("Graphic: " + graphic.path + " | suffix: " + suffix);
                        path += "/" + suffix;
                        graphicInt2 = GraphicDatabase.Get(typeof(Graphic_Single), path, def.graphicData2.shaderType.Shader, def.graphicData2.drawSize, def.graphicData2.color, def.graphicData2.colorTwo);
                    }
                    else
                    {
                        graphicInt2 = this.def.graphicData2.GraphicColoredFor(this);
                    }
                }
                return graphicInt2;
			}
        }

        private void Print(Graphic graphic, SectionLayer layer)
        {
			Vector3 a = this.TrueCenter();
			Rand.PushState();
			Rand.Seed = base.Position.GetHashCode();
			int num = Mathf.CeilToInt(this.growthInt * (float)this.def.plant.maxMeshCount);
			if (num < 1)
			{
				num = 1;
			}
			float num2 = this.def.plant.visualSizeRange.LerpThroughRange(this.growthInt);
			float num3 = graphic.data.drawSize.x * num2;
			Vector3 vector = Vector3.zero;
			int num4 = 0;
			int[] positionIndices = PlantPosIndices.GetPositionIndices(this);
			bool flag = false;
			foreach (int num5 in positionIndices)
			{
				if (this.def.plant.maxMeshCount != 1)
				{
					int num6 = 1;
					int maxMeshCount = this.def.plant.maxMeshCount;
					if (maxMeshCount <= 4)
					{
						if (maxMeshCount != 1)
						{
							if (maxMeshCount != 4)
							{
								goto IL_157;
							}
							num6 = 2;
						}
						else
						{
							num6 = 1;
						}
					}
					else if (maxMeshCount != 9)
					{
						if (maxMeshCount != 16)
						{
							if (maxMeshCount != 25)
							{
								goto IL_157;
							}
							num6 = 5;
						}
						else
						{
							num6 = 4;
						}
					}
					else
					{
						num6 = 3;
					}
					IL_16D:
					float num7 = 1f / (float)num6;
					vector = base.Position.ToVector3();
					vector.y = this.def.Altitude;
					vector.x += 0.5f * num7;
					vector.z += 0.5f * num7;
					int num8 = num5 / num6;
					int num9 = num5 % num6;
					vector.x += (float)num8 * num7;
					vector.z += (float)num9 * num7;
					float max = num7 * 0.3f;
					vector += Gen.RandomHorizontalVector(max);
					goto IL_20B;
					IL_157:
					Log.Error(this.def + " must have plant.MaxMeshCount that is a perfect square.", false);
					goto IL_16D;
				}
				vector = a + Gen.RandomHorizontalVector(0.05f);
				float num10 = (float)base.Position.z;
				if (vector.z - num2 / 2f < num10)
				{
					vector.z = num10 + num2 / 2f;
					flag = true;
				}
				IL_20B:
				bool @bool = Rand.Bool;
				Material matSingle = graphic.MatSingleFor(this);
                Material matSingle2 = OverlayGraphic?.MatSingleFor(this);
				PlantUtility.SetWindExposureColors(colors, this);
				Vector2 size = new Vector2(num3, num3);
				Printer_Plane.PrintPlane(layer, vector, size, matSingle, 0f, @bool, null, colors, 0.1f, (float)(this.HashOffset() % 1024));
				if(matSingle2 != null)
                    Printer_Plane.PrintPlane(layer, vector + new Vector3(0,0.001f,0), size, matSingle2, 0f, @bool, null, colors, 0.1f, (float)(this.HashOffset() % 1024));
				num4++;
				if (num4 >= num)
				{
					break;
				}
			}
			if (graphic.data.shadowData != null)
			{
				Vector3 center = a + graphic.data.shadowData.offset * num2;
				if (flag)
				{
					center.z = base.Position.ToVector3Shifted().z + graphic.data.shadowData.offset.z;
				}
				center.y -= 0.042857144f;
				Vector3 volume = graphic.data.shadowData.volume * num2;
				Printer_Shadow.PrintShadow(layer, center, volume, Rot4.North);
			}
			Rand.PopState();
		}

        public override void Print(SectionLayer layer)
        {
			base.Print(layer);
			//Print(Graphic, layer);
        }
    }
}
