﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class TiberiumGeyserCrack : TRBuilding
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            foreach (var cell in this.OccupiedRect())
            {
                Map.terrainGrid.SetTerrain(cell, TiberiumTerrainDefOf.TiberiumSoilGreen);
            }
        }
    }
}
