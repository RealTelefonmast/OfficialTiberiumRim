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
        public Dictionary<Comp_Suppression, HashSet<IntVec3>> Suppressors = new Dictionary<Comp_Suppression, HashSet<IntVec3>>();

        public SuppressionGrid SuppressionGrid;

        public MapComponent_Suppression(Map map) : base(map)
        {
            SuppressionGrid = new SuppressionGrid(map);
        }

        public bool IsInSuppressorField(IntVec3 cell)
        {
            return SuppressionGrid.GetCellBool(map.cellIndices.CellToIndex(cell));
        }

        public bool IsInSuppressorField(IntVec3 cell, out List<Comp_Suppression> suppressors)
        {
            suppressors = Suppressors.Where(k => k.Value.Contains(cell)).Select(k => k.Key).ToList();
            return !suppressors.NullOrEmpty();
        }

        public void RegisterOrUpdateSuppressor(Comp_Suppression suppressor, List<IntVec3> cells)
        {
            if (Suppressors.ContainsKey(suppressor))
            {
                var list = Suppressors[suppressor];
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var v = list.ElementAt(i);
                    if (cells.Contains(v))
                        cells.Remove(v);
                    else
                    {
                        list.Remove(v);
                        SuppressionGrid.Set(v, false);
                    }
                }
                foreach (var v in cells)
                {
                    list.Add(v);
                    SuppressionGrid.Set(v, true);
                }
            }
            else
            {
                Suppressors.Add(suppressor, new HashSet<IntVec3>(cells));
                foreach (var v in cells)
                {
                    SuppressionGrid.Set(v, true);
                }
            }
        }

        public void DeregisterSuppressor(Comp_Suppression suppressor)
        {
            if (!Suppressors.ContainsKey(suppressor)) return;
            foreach (var v in Suppressors[suppressor])
            {
                SuppressionGrid.Set(v, false);
            }
            Suppressors.Remove(suppressor);
        }
    }
}
