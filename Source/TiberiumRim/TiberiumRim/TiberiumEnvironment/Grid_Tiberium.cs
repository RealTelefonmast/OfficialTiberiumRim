using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    /* Tiberium Grid, keeps track of all cells related to Tiberium
     * Determines growth, inhibition, patterns
     */

    public class TiberiumGrid : ICellBoolGiver, IExposable
    {
        public Map map;

        //Tiberium Exists At These Cells
        public BoolGrid tiberiumGrid; 

        //Tiberium May Grow From These Cells
        public BoolGrid growFromGrid;
        //Tiberium May Grow To These Cells
        public BoolGrid growToGrid;

        //Tiberium Affects These Cells
        public BoolGrid affectedCells;

        public BoolGrid[] fieldColorGrids;

        public TiberiumCrystal[] TiberiumCrystals;
        //TODO: Use int counter on cells to see how many crystals require it, for easier removal of spread ranged pos's
        //public int[] WantedCount;

        public CellBoolDrawer drawer;

        public TiberiumGrid(){}

        public TiberiumGrid(Map map)
        {
            this.map = map;
            tiberiumGrid = new BoolGrid(map);
            growFromGrid  = new BoolGrid(map);
            growToGrid    = new BoolGrid(map);
            affectedCells = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);

            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
            //WantedCount = new int[map.cellIndices.NumGridCells];
        }

        public void ExposeData()
        {
        }

        private int Index(IntVec3 c)
        {
            return map.cellIndices.CellToIndex(c);
        }

        //CellBoolGiver
        public bool GetCellBool(int index)
        {
            return true; //tiberiumGrid[index] || affectedCells[index];
        }

        public Color Color => Color.white;

        public void UpdateAll(IntVec3 c, TiberiumCrystal crystal)
        {
            var cells = c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateEffects(c, crystal, cells);
            UpdateGrow(c, crystal, cells);
        }

        public void UpdateEffects(IntVec3 c, TiberiumCrystal crystal, List<IntVec3> cells = null)
        {
            cells ??= c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            foreach (var cell in cells)
            {
                SetAffectedBool(cell);
            }
        }

        public void UpdateGrow(IntVec3 c, TiberiumCrystal crystal, List<IntVec3> cells = null)
        { 
            cells ??= c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateGrowTo(cells, crystal);
            foreach (var cell in cells)
            {
                SetGrowFromBool(cell);
                SetGrowToGeneric(cell);
            }
        }

        public void Update(IntVec3 c, TiberiumCrystal crystal)
        {
            var cells = c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateGrowTo(cells, crystal);
            Update(cells);
        }

        private void Update(List<IntVec3> cells)
        {
            foreach (var cell in cells)
            {
                SetAffectedBool(cell);
                SetGrowFromBool(cell);
                SetGrowToGeneric(cell);
            }
        }

        private void UpdateGrowTo(List<IntVec3> cells, TiberiumCrystal crystal)
        {
            var potentialGrowTo = crystal != null
                ? cells.Where(t => !(t.HasTiberium(map) || t.HasTibFlora(map))).ToList()
                : cells;//cells.Where(t => growToGrid[t]).ToList();

            if (!potentialGrowTo.Any()) return;
            if (Rand.Chance(crystal?.def.props.rootNodeChance ?? 1f))
                potentialGrowTo.ForEach(SetGrowToSpecific);
            else
                SetGrowToSpecific(WeightedGrowToCell(crystal,potentialGrowTo));
        }

        private IntVec3 WeightedGrowToCell(TiberiumCrystal origin, List<IntVec3> potentialCells)
        {
            Func<IntVec3, float> action = delegate(IntVec3 cell)
            {
                return Mathf.Lerp(1,0, origin.Position.DistanceTo(cell)-1);
            };
            return potentialCells.RandomElementByWeight(action);
        }

        public Color GetCellExtraColor(int index)
        {
            Color color = Color.clear;
            if (growToGrid[index])
            {
                color += Color.cyan;
            }
            if (affectedCells[index])
            {
                color += Color.magenta;
            }
            if (growFromGrid[index])
            {
                color += Color.green;
            }
            return color;
        }

        //TODO: Split dirty calls into tiberium stages for less redudancy
        public void MarkDirty(IntVec3 c, TiberiumCrystal from)
        {
            if (!tiberiumGrid[c] && from.Spawned)
            {
                SetCrystal(c, true, from);
                return;
            }
            Update(c, from);
        }

        public void SetInit()
        {

        }

        public void SetCrystal(IntVec3 c, bool value, TiberiumCrystal crystal)
        {
            TiberiumCrystals[Index(c)] = crystal;
            tiberiumGrid.Set(c, value);

            SetHealthAffects(c);

            Update(c, crystal);
        }

        public void SetFieldColor(IntVec3 c, bool value, TiberiumValueType type)
        {
            switch (type)
            {
                case TiberiumValueType.Green:
                    fieldColorGrids[0][c] = value;
                    break;
                case TiberiumValueType.Blue:
                    fieldColorGrids[1][c] = value;
                    break;
                case TiberiumValueType.Red:
                    fieldColorGrids[2][c] = value;
                    break;
                default:
                    return;
            }
        }

        public void SetHealthAffects(IntVec3 c)
        {
            TiberiumCrystal crystal = TiberiumCrystals[Index(c)];
            
            if(crystal?.def.IsInfective ?? true)
                map.Tiberium().TiberiumAffecter.SetInfection(c, tiberiumGrid[c] ? 1 : -1);
            if (crystal?.def.props.radiates ?? true)
                map.Tiberium().TiberiumAffecter.SetRadiation(c, tiberiumGrid[c] ? 1 : -1);
        }

        private void SetGrowFromBool(IntVec3 c)
        {
            TiberiumCrystal crystal = TiberiumCrystals[Index(c)];
            if (crystal == null)
            {
                growFromGrid[c] = false;
                return;
            }

            growFromGrid[c] = !c.CellsAdjacent8Way().All(v => v.InBounds(map) && (!growToGrid[v] || tiberiumGrid[v] || v.HasTibFlora(map)));
            //growFromGrid[c] &= !crystal.OutOfParentRange;

            //bool surrounded = c.CellsAdjacent8Way().All(v => v.InBounds(map) && (tiberiumGrid[v] || map.Tiberium().FloraInfo.HasFloraAt(v))); //!c.CellsAdjacent8Way().Any(v => v.InBounds(map) && (!tiberiumBools[v] && !map.Tiberium().FloraInfo.HasFloraAt(v)));
            //growFromGrid[c] &= !surrounded;
        }

        private void SetGrowToSpecific(IntVec3 c)
        {
            growToGrid[c] = c.CellsAdjacent8Way().Any(t => t.InBounds(map) && tiberiumGrid[t]);
        }

        private void SetGrowToGeneric(IntVec3 c)
        {
            //Main Ruleset For Tiberium Spread
            growToGrid[c] = growToGrid[c];
            growToGrid[c] &= !tiberiumGrid[c];
        }

        private void SetAffectedBool(IntVec3 c)
        {
            affectedCells[c] = !tiberiumGrid[c] && c.CellsAdjacent8Way().Any(v => v.InBounds(map) && tiberiumGrid[v]);
            //if(affectedCells[c])
                //map.Tiberium().cell
        }

        public void Notify_ThingUpdated(Thing thing)
        {

        }

        public void Notify_TerrainUpdated(TerrainDef def, IntVec3 cell)
        {

        }



        //
        //First Step - Generic Checkup, static objects in the way?
    }
}