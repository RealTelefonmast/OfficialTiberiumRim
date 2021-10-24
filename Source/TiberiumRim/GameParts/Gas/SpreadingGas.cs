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
        public int maxSaturation = 10000;
        public int minSpreadSaturation = 1;
        //public int dissipationSaturation = 10;

        public NetworkValueDef dissipateTo;
    }

    public class SpreadingGas : Gas
    {
        public new TRThingDef def;
        private TRGasProperties props;

        private int spawnTick;

        private int tickOffset;
        private int saturation;
        private int overflowValue;

        private float curSaturationPct;

        private IntVec3[] randomCells;

        private readonly SimpleCurve OpacityCurve = new SimpleCurve()
        {
            new(0,0),
            new(0.25f,0.45f),
            new(0.5f,0.65f),
            new(0.75f,0.75f),
            new(1f,1f),
        };

        //[TweakValue("SPREAD_GAS_VISCOSITY", 0, 1)]
        //public static float Viscosity = 0.5f;

        public int DissipationAmount => props.maxSaturation / 100;

        public override string LabelCap => $"{base.LabelCap} ({curSaturationPct.ToStringPercent()})({((overflowValue)/(float)props.maxSaturation).ToStringPercent()})";

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            props = def.gasProps;

            spawnTick = Find.TickManager.TicksGame;
            tickOffset = Rand.Range(0, 125);
            randomCells = GenAdjFast.AdjacentCellsCardinal(Position).InRandomOrder().ToArray();
            SetRandValues();
        }

        public override void Tick()
        {
            if (mapIndexOrState < 0) return;

            curSaturationPct = saturation / (float) props.maxSaturation;
            graphicRotation += graphicRotationSpeed;

            if (curSaturationPct <= 0.001953125f)
            {
                DeSpawn();
                return;
            }

            spawnTick++;
            if (((spawnTick + tickOffset) % 125) != 0) return;

            DoGasCellEffect();

            if (!CanSpreadTo(Position, out _))   //cloud is in inaccessible cell, probably a recently closed door or vent. Spread to nearby cells and delete.
            {
                TrySpread();
                DeSpawn();
                return;
            }

            var room = this.GetRoom();
            if (room != null)
            {
                var pollution = Map.Tiberium().AtmosphericInfo.ComponentAt(room);
                if (!pollution.UsedContainer.FullySaturated)
                {
                    //!Position.Roofed(Map) &&
                    AdjustSaturation(-DissipationAmount, out var actualValue);
                    if (props.dissipateTo != null)
                    {
                        pollution.TryAddValue(props.dissipateTo, -(actualValue / 10), out _);
                    }
                }
            }
            TrySpread();
        }

        public virtual void DoHealthEffect()
        {

        }

        protected virtual void DoGasCellEffect()
        {
        }

        //
        private void TrySpread()
        {
            if (saturation < props.minSpreadSaturation) return;

            foreach (var cell in randomCells)
            {
                if (!CanSpreadTo(cell, out float passPct)) continue;

                if (cell.GetGas(Map) is SpreadingGas gas)
                {
                    if (gas.curSaturationPct > curSaturationPct) continue;
                    var diff = saturation - gas.saturation;
                    EqualizeWith(gas, (int)((diff * 0.5f) * (passPct)));
                }
                else
                {
                    var newGas = (SpreadingGas)GenSpawn.Spawn(this.def, cell, Map);
                    EqualizeWith(newGas, (int)((saturation * 0.5f) * (passPct)));
                }
            }
        }

        private bool CanSpreadTo(IntVec3 other, out float passPct)
        {
            passPct = 0f;
            if (!other.InBounds(Map)) return false;
            passPct = other.GetFirstBuilding(Map)?.AtmosphericPassPercent() ?? 1f;
            return passPct > 0;
        }

        public void EqualizeWith(SpreadingGas other, int value)
        {
            AdjustSaturation(-value, out int actualValue);
            other.AdjustSaturation(-actualValue, out _);
        }

        public void AdjustSaturation(int value, out int actualValue)
        {
            actualValue = value;
            var val = saturation + value;
            if (overflowValue > 0 && val < props.maxSaturation)
            {
                var extra = Mathf.Clamp(props.maxSaturation - val, 0, overflowValue);
                val += extra;
                overflowValue -= extra;
            }
            saturation = Mathf.Clamp(val, 0, props.maxSaturation);
            if (val < 0)
            {
                actualValue = value + val;
                return;
            }

            if (val < props.maxSaturation) return;
            var overFlow = val - props.maxSaturation;
            actualValue = value - overFlow;
            overflowValue += overFlow;

            // Recursive equalization
            /*
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
            */
        }

        private MaterialPropertyBlock propertyBlock;

        private int randAngle = 0;
        private float randRangePos1 = 0;
        private float randRangePos2 = 0;

        private Vector3 randSize;

        private void SetRandValues()
        {
            randAngle = Rand.Range(0, 360);
            randRangePos1 = Rand.Range(-0.45f, 0.45f);
            randRangePos2 = Rand.Range(0.8f, 1.2f);
            randSize = new Vector3(randRangePos2 * def.graphicData.drawSize.x, 0f, randRangePos2 * def.graphicData.drawSize.y); ;
        }

        public override void Draw()
        {
            propertyBlock ??= new MaterialPropertyBlock();

            var alpha = (0.1f + Mathf.Lerp(0, 0.9f, curSaturationPct));
            propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(1, 1, 1, alpha));

            float angle = randAngle + this.graphicRotation;
            Vector3 pos = DrawPos + new Vector3(randRangePos1, 0f, randRangePos1);

            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.AngleAxis(angle, Vector3.up), randSize), Graphic.MatSingle, 0, null, 0, propertyBlock);
        }
    }
}
