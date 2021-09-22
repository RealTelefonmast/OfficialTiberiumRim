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
        public int maxSaturation = 10000;
        public int minSpreadSaturation = 1;
        //public int dissipationSaturation = 10;

        public NetworkValueDef dissipateTo;
    }

    public class SpreadingGas : Gas
    {
        public new TRThingDef def;

        private int tickOffset;
        private int saturation;

        private IntVec3[] randomCells;

        private readonly SimpleCurve OpacityCurve = new SimpleCurve()
        {
            new(0,0),
            new(0.25f,0.45f),
            new(0.5f,0.65f),
            new(0.75f,0.75f),
            new(1f,1f),
        };

        [TweakValue("SPREAD_GAS_VISCOSITY", 0, 1)]
        public static float Viscosity = 0.5f;

        public TRGasProperties Props => def.gasProps;

        public float SaturationPercent => saturation / (float)Props.maxSaturation;
        public int DissipationAmount => Props.maxSaturation / 100;
        //public bool Dissipating => saturation <= Props.dissipationSaturation;

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
            randomCells = GenAdjFast.AdjacentCellsCardinal(Position).InRandomOrder().ToArray();
            //saturation = Rand.Range(Props.maxSaturation / 2, Props.maxSaturation);
        }

        public override void Tick()
        {
            if (!Spawned) return;

            graphicRotation += graphicRotationSpeed;
            if (SaturationPercent <= 0.0001f)
            {
                Destroy();
                return;
            }

            var curTick = Find.TickManager.TicksGame;
            if (((curTick + tickOffset) % Props.updateInterval) != 0) return;

            if (!CanSpreadTo(Position, out _))   //cloud is in inaccessible cell, probably a recently closed door or vent. Spread to nearby cells and delete.
            {
                TrySpread();
                Destroy();
                return;
            }

            var room = this.GetRoom();
            if (room != null)
            {
                var pollution = Map.Tiberium().AtmosphericInfo.PollutionFor(room);
                if (!pollution.UsedContainer.FullySaturated)
                {
                    //!Position.Roofed(Map) &&
                    AdjustSaturation(-DissipationAmount, out var actualValue);
                    if (Props.dissipateTo != null)
                    {
                        pollution.TryAddValue(Props.dissipateTo, -(actualValue / 10), out _);
                    }
                }
            }

            TrySpread();
        }

        public void AdjustSaturation(int value, out int actualValue)
        {
            actualValue = value;
            var val = saturation + value;
            saturation = Mathf.Clamp(val, 0, Props.maxSaturation);

            if (val < 0)
            {
                actualValue = value + val;
                return;
            }

            if (val < Props.maxSaturation) return;
            var overFlow = val - Props.maxSaturation;
            actualValue = value - overFlow;

            foreach (var cell in randomCells)
            {
                if (!CanSpreadTo(cell, out _)) continue;

                if (cell.GetGas(Map) is SpreadingGas gas)
                {
                    if (gas.SaturationPercent >= SaturationPercent) continue;
                    gas.AdjustSaturation(overFlow, out _);
                    return;
                }
                var newGas = (SpreadingGas)GenSpawn.Spawn(this.def, cell, Map);
                newGas.AdjustSaturation(overFlow, out _);
                return;
            }
        }


        public void EqualizeWith(SpreadingGas other, int value)
        {
            AdjustSaturation(-value, out int actualValue);
            other.AdjustSaturation(-actualValue, out _);
        }

        //
        private void TrySpread()
        {
            if (saturation < Props.minSpreadSaturation) return;

            foreach (var cell in randomCells)
            {
                if (!CanSpreadTo(cell, out float passPct)) continue;

                if (cell.GetGas(Map) is SpreadingGas gas)
                {
                    if (gas.SaturationPercent > SaturationPercent) continue;
                    var diff = saturation - gas.saturation;
                    EqualizeWith(gas, (int) ((diff * 0.25f) * (passPct * Viscosity)));
                }
                else
                {
                    var newGas = (SpreadingGas) GenSpawn.Spawn(this.def, cell, Map);
                    EqualizeWith(newGas, (int) ((saturation * 0.25f) * (passPct * Viscosity)));
                }
            }
        }

        private bool CanSpreadTo(IntVec3 other, out float passPct)
        {
            passPct = 1f;
            if (!other.InBounds(Map)) return false;
            passPct = other.GetFirstBuilding(Map)?.AtmosphericPassPercent() ?? 1f;
            return passPct > 0;
        }

        private MaterialPropertyBlock propertyBlock;

        public override void Draw()
        {
            propertyBlock ??= new MaterialPropertyBlock();

            Rand.PushState();
            Rand.Seed = this.thingIDNumber.GetHashCode();

            var alphaRaw = (0.1f + Mathf.Lerp(0, 0.9f, SaturationPercent));
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
