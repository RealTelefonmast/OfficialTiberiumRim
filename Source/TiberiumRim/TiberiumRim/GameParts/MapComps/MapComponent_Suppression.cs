using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_Suppression : MapComponent
    {
        //public Dictionary<Comp_Suppression, List<IntVec3>> Suppressors = new Dictionary<Comp_Suppression, List<IntVec3>>();
        public List<Comp_Suppression> Suppressors = new List<Comp_Suppression>();
        private readonly SuppressionGrid grid;
        private readonly List<Comp_Suppression> dirtySuppressors = new List<Comp_Suppression>();

        public MapComponent_Suppression(Map map) : base(map)
        {
            grid = new SuppressionGrid(map);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            UpdateDirties();
        }

        public IEnumerable<IntVec3> ActiveCells => grid.suppressionBools.ActiveCells;

        public bool IsInSuppressorField(IntVec3 cell)
        {
            return grid.suppressionBools[cell]; //.GetCellBool(map.cellIndices.CellToIndex(cell));
        }

        public bool IsInSuppressorField(IntVec3 cell, out List<Comp_Suppression> suppressors)
        {
            suppressors = Suppressors.Where(s => s.SuppressionCells.Contains(cell)).ToList();
            return suppressors.Count > 0;
        }

        public void UpdateSuppressors()
        {
            foreach (var suppressor in Suppressors)
            {
                UpdateGrid(suppressor);
            }
        }

        public void UpdateGrid(Comp_Suppression suppressor)
        {
            RemoveFromGrid(suppressor.SuppressionCells);
            suppressor.UpdateSuppressionCells();
            AddToGrid(suppressor.SuppressionCells);
        }

        public void RegisterSuppressor(Comp_Suppression suppressor)
        {
            Suppressors.Add(suppressor);
        }

        public void DeregisterSuppressor(Comp_Suppression suppressor)
        {
            Suppressors.Remove(suppressor);
        }

        public void RemoveFromGrid(List<IntVec3> cells)
        {
            foreach (var cell in cells)
            {
                grid.Set(cell, false);
            }
        }

        public void AddToGrid(List<IntVec3> cells)
        {
            foreach (var cell in cells)
            {
                grid.Set(cell, true);
            }
        }

        public void MarkDirty(List<Comp_Suppression> suppressors)
        {
            dirtySuppressors.AddRange(suppressors);
        }

        public void MarkDirty(Comp_Suppression suppresor)
        {
            dirtySuppressors.Add(suppresor);
        }

        public void UpdateDirties()
        {
            for (var index = dirtySuppressors.Count - 1; index >= 0; index--)
            {
                var suppressor = dirtySuppressors[index];
                UpdateGrid(suppressor);
                dirtySuppressors.Remove(suppressor);
            }
        }
    }
}
