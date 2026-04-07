// Preserved from TeleCore/SpreadingGas/SpreadingGasGrid.cs
using System;
using System.Runtime.CompilerServices;
using TeleCore;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.OldRef
{
    //TODO: Make rule based sprading and dissipation: ie: if a gas cell is surrounded by gas of the same type, it spreads less than a cell with free neighbours => viscosity simulation
    public unsafe class SpreadingGasGrid_TAE : MapInformation
    {
        internal static TAE.SpreadingGasTypeDef[] GasDefsArr;
        internal static int GasDefsCount;
        internal const int AlphaCurvePoints = 8;
        internal const int AlphaCurvePointsData = AlphaCurvePoints + 1; //TODO use a n+1th position to set max size to not have comparisions against "empty" curve points

        //
        private TAE.SpreadingGasGridRenderer renderer;
        private TAE.DynamicDataCacheMapInfo cacheMapInfo;

        //Map Data
        private int gridSize;

        //
        public readonly UnityEngine.Color[] minColors;
        public readonly UnityEngine.Color[] maxColors;
        public readonly uint[] maxDensities;

        //
        private NativeArray<TAE.GasCellStack> gasGridData;
        private readonly TAE.GasCellStack* gasGridPtr;

        //
        private readonly int[] totalSubGasCount;
        private readonly int[] totalSubGasValue;
        private uint totalGasCount;
        private long totalGasValue;

        //Spreading And Dissipation
        private int workingIndex = 0;
        private readonly int workingCellCount;
        private readonly byte[] randomSpreadDirs;

        //
        public NativeArray<TAE.GasCellStack> GasGrid => gasGridData;
        public int Length => gasGridData.Length;

        //Value Tracking
        public int[] TotalSubGasCount => totalSubGasCount;
        public int[] TotalSubGasValue => totalSubGasValue;
        public uint TotalGasCount => totalGasCount;
        public long TotalGasValue => totalGasValue;

        private TAE.DynamicDataCacheMapInfo CacheMapInfo => cacheMapInfo ??= Map.GetMapInfo<TAE.DynamicDataCacheMapInfo>();

        //
        public bool HasAnyGas => totalGasCount > 0;

        //
        public SpreadingGasGrid_TAE(Map map) : base(map)
        {
            gridSize = map.cellIndices.NumGridCells;

            //
            if (GasDefsArr == null)
            {
                GasDefsArr = DefDatabase<TAE.SpreadingGasTypeDef>.AllDefsListForReading.ToArray();
                GasDefsCount = GasDefsArr.Length;
            }

            minColors = new UnityEngine.Color[GasDefsCount];
            maxColors = new UnityEngine.Color[GasDefsCount];
            maxDensities = new uint[GasDefsCount];

            for (int i = 0; i < GasDefsCount; i++)
            {
                minColors[i] = GasDefsArr[i].colorMin;
                maxColors[i] = GasDefsArr[i].colorMax;
                maxDensities[i] = (uint)GasDefsArr[i].maxDensityPerCell;
            }

            //
            renderer = new TAE.SpreadingGasGridRenderer(null, map);

            gasGridData = new NativeArray<TAE.GasCellStack>(gridSize, Allocator.Persistent);
            gasGridPtr = (TAE.GasCellStack*)gasGridData.GetUnsafePtr();

            for (var c = 0; c < gridSize; c++)
            {
                gasGridPtr[c] = new TAE.GasCellStack();
            }

            totalSubGasCount = new int[GasDefsCount];
            totalSubGasValue = new int[GasDefsCount];

            //
            randomSpreadDirs = new byte[] {0, 1, 2, 3};
            randomSpreadDirs.Shuffle();

            //
            workingCellCount = 128;
        }

        public override void ExposeDataExtra()
        {
        }

        public void Notify_SpawnGasAt(IntVec3 cell, TAE.SpreadingGasTypeDef gasType, float value)
        {
            TryAddGasAt_Internal(cell, gasType, (ushort)value);
        }

        public override void Tick()
        {
            //
        }

        public override void TeleTick()
        {
            if (!HasAnyGas) return;

            int area = map.Area;
            var cellsInRandomOrder = map.cellsInRandomOrder.GetAll();

            for (int i = 0; i < workingCellCount; ++i)
            {
                if (workingIndex >= area)
                    workingIndex = 0;

                var cell = cellsInRandomOrder[workingIndex];

                for (int id = 0; id < GasDefsCount; ++id)
                {
                    TrySpreadGas(cell, id);
                    Dissipate(cell.Index(map), cell , id);
                }
                workingIndex++;
            }
        }

        private void Dissipate(int index, IntVec3 cell, int defID)
        {
            if (index < 0 || index >= Length || defID >= GasDefsCount)
            {
                TLog.Warning($"Index for gasGrid cell is out of bound: {index} | {defID}");
                return;
            }

            var cellValue = gasGridPtr[index][defID];
            if (cellValue.totalBitVal == 0) return;
            if (cellValue.value == 0) return;
            var def = ((TAE.SpreadingGasTypeDef)defID);

            if (((TAE.SpreadingGasTypeDef) defID).roofBlocksDissipation && cell.Roofed(map))
            {
                if (def.dissipateTo != null)
                {
                    var room = cell.GetRoomFast(map);
                    var roomComp = room.GetRoomComp<TAE.RoomComponent_Atmospheric>();
                    if (room is {ProperRoom: true} && roomComp.Notify_SpradingGasDissipating(def, def.dissipationAmount, out var actual))
                    {
                        SetDensity_Direct(index, defID, (ushort)Math.Max(cellValue.value - actual.ActualAmount, 0));
                    }
                }
                return;
            }

            cellValue.value = (ushort)Math.Max(cellValue.value - def.dissipationAmount, 0);
            SetDensity_Direct(index, defID, cellValue.value);
        }

        void TrySpreadGas(IntVec3 pos, int defID)
        {
            var index = pos.Index(map);
            var def = ((TAE.SpreadingGasTypeDef)defID);
            var cellValue = CellValueAtUnsafe(index, defID);

            if (cellValue.overflow > 0)
            {
                var extra = (ushort) Mathf.Clamp(cellValue.overflow, 0, def.maxDensityPerCell);
                cellValue.value += extra;
                cellValue.overflow -= extra;
            }

            if (cellValue.value == 0) return;
            if (cellValue.value < def.minSpreadDensity) return;

            for (int i = 0; i < randomSpreadDirs.Length; i++)
            {
                var offset = IndexOffset(index, randomSpreadDirs[i]);
                if (!CanSpreadTo(offset, def, out float passPct)) continue;

                int newIndex = offset;
                var cellValueNghb = CellValueAtUnsafe(newIndex, defID);

                if (TryEqualizeWith(ref cellValue, ref cellValueNghb, def, passPct))
                {
                    SetCellValueAt(index, cellValue);
                    SetCellValueAt(newIndex, cellValueNghb);
                }
            }
        }

        private void SetCellValueAt(int index, TAE.GasCellValue value)
        {
            DetectValueChange(value.defID, gasGridPtr[index][value.defID].value, value.value);
            var previousTotal = gasGridPtr[index].totalValue;
            var val = gasGridPtr[index];
            val[value.defID] = value;
            gasGridPtr[index] = val;
            DetectCountChange(previousTotal, gasGridPtr[index].totalValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TAE.GasCellValue CellValueAtUnsafe(int index, int defID)
        {
            return gasGridPtr[index].stackPtr[defID];
        }

        private void SetDensity_Direct(int index, int defID, ushort value)
        {
            var cellValue = gasGridPtr[index][defID];
            cellValue.value = value;
            SetCellValueAt(index, cellValue);
        }

        private void DetectValueChange(int defID, ushort previous, ushort value)
        {
            var calcVal = value - previous;
            totalSubGasValue[defID] += calcVal;
            totalGasValue += calcVal;

            switch (value)
            {
                case > 0 when previous <= 0:
                    totalSubGasCount[defID]++;
                    break;
                case <= 0 when previous > 0:
                    totalSubGasCount[defID]--;
                    break;
            }
        }

        private void DetectCountChange(long previousTotal, long newTotal)
        {
            switch (newTotal)
            {
                case > 0 when previousTotal <= 0:
                    totalGasCount++;
                    break;
                case <= 0 when previousTotal > 0:
                    totalGasCount--;
                    break;
            }
        }

        private bool OutOfBounds(int index)
        {
            return index < 0 || index >= gridSize;
        }

        private int IndexOffset(int index, int direction)
        {
            switch (direction)
            {
                case Rot4.NorthInt: index += map.cellIndices.mapSizeX; break;
                case Rot4.EastInt: index += 1; break;
                case Rot4.SouthInt: index -= map.cellIndices.mapSizeX; break;
                case Rot4.WestInt: index -= 1; break;
            }
            return index;
        }

        private static bool TryEqualizeWith(ref TAE.GasCellValue gasCellA, ref TAE.GasCellValue gasCellB, TAE.SpreadingGasTypeDef def, float passPct)
        {
            float diff = (gasCellA.value - gasCellB.value);
            if (diff <= 0) return false;
            var diffShort = (ushort)(Mathf.Abs(diff * passPct) * 0.35 * def.ViscosityMultiplier);
            gasCellA -= diffShort;
            gasCellB += diffShort;
            return true;
        }

        private static void AdjustSaturation(ref TAE.GasCellValue cellValue, TAE.SpreadingGasTypeDef def, int value, out int actualValue)
        {
            actualValue = value;
            var val = cellValue.value + value;
            cellValue.value = (ushort)Mathf.Clamp(val, 0, def.maxDensityPerCell);
            if (val < 0) { actualValue = value + val; return; }
            if (val < def.maxDensityPerCell) return;
            var overFlow = val - def.maxDensityPerCell;
            actualValue = value - overFlow;
            cellValue.overflow = (ushort)(cellValue.overflow + overFlow);
        }

        private bool CanSpreadTo(int otherIndex, TAE.SpreadingGasTypeDef forDef, out float passPct)
        {
            passPct = 0f;
            if (OutOfBounds(otherIndex)) return false;
            if (gasGridPtr[otherIndex][forDef].value >= forDef.maxDensityPerCell) return false;
            passPct = CacheMapInfo.AtmosphericPassGrid[otherIndex];
            return passPct > 0;
        }

        private void TryAddGasAt_Internal(IntVec3 cell, TAE.SpreadingGasTypeDef gasType, ushort amount, bool noOverflow = false)
        {
            if (!CanSpreadTo(cell.Index(map), gasType, out _)) return;
            int index = CellIndicesUtility.CellToIndex(cell, Map.Size.x);
            var cellValue = gasGridPtr[index][gasType];
            AdjustSaturation(ref cellValue, gasType, amount, out _);
            if (noOverflow) cellValue.overflow = 0;
            SetCellValueAt(index, cellValue);
        }
    }
}
