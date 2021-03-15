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

        public IEnumerable<IntVec3> SuppressedCells => grid.SuppressionBools.ActiveCells;
        public IEnumerable<IntVec3> CoveredCells => grid.CoveredBools.ActiveCells;

        public bool IsSuppressed(IntVec3 cell)
        {
            return grid.SuppressionBools[cell];
        }

        public bool IsCovered(IntVec3 cell)
        {
            return grid.CoveredBools[cell];
        }

        public void RegisterSuppressor(Comp_Suppression suppressor)
        {
            grid.RegisterSource(suppressor);
            MarkDirty(suppressor);
        }

        public void DeregisterSuppressor(Comp_Suppression suppressor)
        {
            grid.DeregisterSource(suppressor);
        }

        public bool IsInSuppressionCoverage(IntVec3 cell, out List<Comp_Suppression> suppressors)
        {
            suppressors = null;
            if (!IsCovered(cell)) return false;
            suppressors = grid.Sources.Where(s => s.CoversCell(cell)).ToList();
            return suppressors.Count > 0;
        }

        public bool IsSuppressed(IntVec3 cell, out List<Comp_Suppression> suppressors)
        {
            suppressors = null;
            if (!IsSuppressed(cell)) return false;
            suppressors = grid.Sources.Where(s => s.AffectsCell(cell)).ToList();
            return suppressors.Count > 0;
        }

        public void UpdateSuppressor(Comp_Suppression suppressor)
        {
            Toggle(suppressor, false);
            var oldCells = new List<IntVec3>(suppressor.SuppressionCells);
            suppressor.UpdateSuppressionCells();
            grid.Notify_SuppressionSourceUpdated(suppressor, oldCells);
            Toggle(suppressor, true);
        }

        public void Toggle(Comp_Suppression suppressor, bool toggleOn)
        {
            grid.ToggleSuppressor(suppressor, toggleOn);
        }


        public void MarkDirty(List<Comp_Suppression> suppressors)
        {
            dirtySuppressors.AddRange(suppressors);
        }

        public void MarkDirty(Comp_Suppression suppressor)
        {
            dirtySuppressors.Add(suppressor);
        }

        public void UpdateDirties()
        {
            if (!dirtySuppressors.Any()) return;
            for (var index = dirtySuppressors.Count - 1; index >= 0; index--)
            {
                var suppressor = dirtySuppressors[index];
                dirtySuppressors.Remove(suppressor);
                if (suppressor.parent.DestroyedOrNull()) continue;
                UpdateSuppressor(suppressor);
            }
        }
    }
}
