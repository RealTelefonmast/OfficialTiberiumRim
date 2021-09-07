using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    // Spreading gas inspired by CE Smoke/VFE Helixien, heavily edited and reworked
    public class TRGasProperties
    {
        public int updateInterval = 60;
        public float maxSaturation = 10000f;
        public float minSpreadSaturation = 1f;
        public float dissipationSaturation = 10f;

        public NetworkValueDef dissipatesTo;
    }

    public class SpreadingGas : Gas
    {
        public new TRThingDef def;

        private int tickOffset;
        private float saturation;

        private readonly SimpleCurve OpacityCurve = new SimpleCurve()
        {
            new(0,0),
            new(0.25f,0.45f),
            new(0.5f,0.65f),
            new(0.75f,0.75f),
            new(1f,1f),
        };

        public TRGasProperties Props => def.gasProps;

        public float SaturationPercent => saturation / Props.maxSaturation;


        [TweakValue("SPREAD_GAS_VISCOSITY", 0, 1)]
        public static float Viscosity = 0.1f;

        public override string LabelCap => $"{base.LabelCap} ({SaturationPercent.ToStringPercent()})";

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            tickOffset = Rand.Range(0, Props.updateInterval);
            //saturation = Rand.Range(Props.maxSaturation / 2, Props.maxSaturation);
        }

        public override void Tick()
        {
            if (!Spawned) return;
            if (saturation > Props.dissipationSaturation)
            {
                destroyTick++;
            }

            if ((GenTicks.TicksGame + tickOffset) % Props.updateInterval != 0) return;
            if (!Position.Roofed(Map)) //Dissipate into pollution grid
            {
                //Map.Tiberium().PollutionInfo.
                //AdjustSaturation(-1);
            }
            TrySpread();

            base.Tick();
        }

        public void AdjustSaturation(float value)
        {
            saturation = Mathf.Clamp(saturation + value, 0, Props.maxSaturation);
        }

        public void EqualizeWith(SpreadingGas other, float value)
        {
            AdjustSaturation(-value);
            other.AdjustSaturation(value);
        }

        private void TrySpread()
        {
            if (saturation < Props.minSpreadSaturation) return;
            
            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(Position).InRandomOrder())
            {
                if (!CanSpreadTo(cell, out float passPct)) continue;

                if (cell.GetGas(Map) is SpreadingGas gas)
                {
                    if (gas.SaturationPercent > SaturationPercent) return;
                    var diff = saturation - gas.saturation;
                    EqualizeWith(gas, (diff / 2f) * (passPct * Viscosity));
                }
                else
                {
                    var newGas = (SpreadingGas) GenSpawn.Spawn(this.def, cell, Map);
                    EqualizeWith(newGas, (saturation / 2f) * (passPct * Viscosity));
                }
            }
        }

        //
        private bool CanSpreadTo(IntVec3 other, out float passPct)
        {
            passPct = other.GetFirstBuilding(Map)?.AtmosphericPassPercent() ?? 1f;
            return other.InBounds(Map) && passPct > 0;
        }

        private MaterialPropertyBlock propertyBlock;

        public override void Draw()
        {
            propertyBlock ??= new MaterialPropertyBlock();

            Rand.PushState();
            Rand.Seed = this.thingIDNumber.GetHashCode();

            var alphaRaw = (0.1f + Mathf.Lerp(0, 0.9f, OpacityCurve.Evaluate(SaturationPercent)));
            //var alpha = Mathf.Round(alphaRaw * 128) / 128;

            float angle = Rand.Range(0, 360) + this.graphicRotation;
            Vector3 pos = DrawPos + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.45f, 0.45f));
            Vector3 s = new Vector3(Rand.Range(0.8f, 1.2f) * def.graphicData.drawSize.x, 0f, Rand.Range(0.8f, 1.2f) * def.graphicData.drawSize.y);

            propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(1,1,1, alphaRaw));
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s), Graphic.MatSingle, 0, null, 0, propertyBlock);

            Rand.PopState();
        }
    }
}
