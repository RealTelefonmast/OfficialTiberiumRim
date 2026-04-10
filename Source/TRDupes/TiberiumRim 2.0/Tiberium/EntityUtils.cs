using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public static class EntityUtils
    {
        public static TiberiumEntity GetTiberiumEntity(this IntVec3 cell, Map map)
        {
            return map.GetComponent<MapComponent_TiberiumEntityManager>().EntityGrid.TiberiumEntities[map.cellIndices.CellToIndex(cell)];
        }

    }
}
