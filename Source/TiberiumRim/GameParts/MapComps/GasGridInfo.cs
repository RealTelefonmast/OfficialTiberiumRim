using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TeleCore;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace TiberiumRim
{
    public class GasGridInfo : MapInformation, ICellBoolGiver
    {
        private ComputeShader shaderInt;
        private ComputeGrid<CellValueData> gasGrid;
        private ComputeBuffer offsetBufferInt;

        public BoolGrid activeGrid;
        public CellBoolDrawer drawer;

        //Settings
        private static readonly int MAX_CELL_VALUE = 1000;
        private static readonly int SPREAD_VALUE = 100;
        private static readonly int RANDOM_ROTATIONS = 8;

        private IntVec3[] randomOffsets;

        private ComputeBuffer PassGridBuffer => map.Tiberium().DynamicDataInfo.AtmosphericBuffer;

        private ComputeBuffer OffsetBuffer
        {
            get
            {
                if (offsetBufferInt == null)
                {
                    offsetBufferInt = new ComputeBuffer(randomOffsets.Length, sizeof(int) * 3);
                    offsetBufferInt.SetData(randomOffsets);
                }
                return offsetBufferInt;
            }
        }

        private ComputeShader MainShader
        {
            get
            {
                if (shaderInt == null)
                {
                    shaderInt = TRContentDatabase.GasGridCompute;
                    Log.Message("Loading buffers in shader...");
                    shaderInt.SetBuffer(0, "gasGrid", gasGrid.DataBuffer);
                    shaderInt.SetBuffer(0, "passData", PassGridBuffer);
                    shaderInt.SetBuffer(0, "offsets", OffsetBuffer);
                    shaderInt.SetInts("MAP_SIZE", map.Size.x, map.Size.y, map.Size.z);
                    shaderInt.SetInt("MIN_SPREAD_VALUE", SPREAD_VALUE);
                    shaderInt.SetInt("MAX_VAL", MAX_CELL_VALUE);
                    shaderInt.SetInt("MAX_ROTS", RANDOM_ROTATIONS);
                }
                return shaderInt;
            }
        }

        public GasGridInfo(Map map) : base(map)
        {
            gasGrid = new ComputeGrid<CellValueData>(map);
            activeGrid = new BoolGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.5f);

            randomOffsets = new IntVec3[RANDOM_ROTATIONS * 8];
            for (int i = 0; i < RANDOM_ROTATIONS; i++)
            {
                var arr = GenAdj.AdjacentCellsAround.InRandomOrder().ToArray();
                for (int k = 0; k < 8; k++)
                {
                    randomOffsets[i * 8 + k] = arr[k];
                }
            }
        }

        public override void SafeInit()
        {
            gasGrid.ThreadSafeInit();
        }

        public void SetValue(IntVec3 c)
        {
            gasGrid.SetValue(c, new CellValueData((uint)c.Index(map), 1000));
        }

        public override void CellSteadyEffect(IntVec3 c)
        {
            //TrySpread(c);
        }

        public override void Tick()
        {
            //CalculateGasGridFrameGPU();
        }

        public void DoSpreadOnce()
        {
            //CalculateGasGridFrameGPU();
        }

        private void CalculateGasGridFrameGPU()
        {
            MainShader.Dispatch(0, gasGrid.Length, 1, 1);
            GameComponent_TR.TRComp().MainRoot.StartCoroutine(GetData());

        }

        private IEnumerator GetData()
        {
            yield return null;
            //GasGridBuffer.GetData(gasGrid.DataArray);
            yield return null;
        }

        /*
        private void TrySpread(IntVec3 c)
        {
            int value = grid[c];
            if (value < spreadValue) return;

            foreach (var cell in AdjacentRandomCells(c))
            {
                if (!CanSpreadTo(cell, out float passPct)) continue;

                var cellVal = grid[cell];

                var diff = value - cellVal;
                EqualizeWith(c, cell, (int) ((diff * 0.25f) * (passPct)));
            }
        }

        private bool CanSpreadTo(IntVec3 other, out float passPct)
        {
            passPct = 0f;
            if (!other.InBounds(Map)) return false;
            passPct = other.GetFirstBuilding(Map)?.AtmosphericPassPercent() ?? 1f;
            return passPct > 0;
        }

        public void EqualizeWith(IntVec3 a, IntVec3 b, int value)
        {
            AdjustSaturation(a, -value, out int actualValue);
            AdjustSaturation(b, -actualValue, out _);
        }

        public void AdjustSaturation(IntVec3 c, int value, out int actualValue)
        {
            var saturation = grid[c];
            actualValue = value;
            var val = saturation + value;

            //Try add overflow
            var overFlowVal = overFlowGrid[c];
            if (overFlowVal > 0)
            {
                if (val < maxVal)
                {
                    var extra = Mathf.Clamp(maxVal - val, 0, overFlowVal);
                    val += extra;
                    overFlowGrid[c] -= extra;
                }
            }

            grid[c] = Mathf.Clamp(val, 0, maxVal);
            activeGrid[c] = grid[c] > 0;

            //Log.Message($"Adjusting {c}: {saturation} + {value} -> {grid[c]}[{activeGrid[c]}]");
            if (val < 0)
            {
                actualValue = value + val;
                //Log.Message($"ActualVal(Neg): {actualValue} from {val}");
                return;
            }

            // Recursive equalization
            if (val <= maxVal) return;
            var overFlow = val - maxVal;
            actualValue = value - overFlow;

            overFlowGrid[c] += overFlow;
        }
        */

        public override void Draw()
        {
            //drawer.SetDirty();
            //drawer.MarkForDraw();
            //drawer.CellBoolDrawerUpdate();
        }

        public bool GetCellBool(int index)
        {
            return true; //activeGrid[index];
        }

        public Color GetCellExtraColor(int index)
        {
            return new Color(0,0,1f, gasGrid[index].value/(float)1000);
            
        }

        public Color Color => Color.white;
    }
}
