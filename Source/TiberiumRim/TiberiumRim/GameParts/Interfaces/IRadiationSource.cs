﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public interface IRadiationSource
    {
        public List<IntVec3> AffectedCells { get; set; }
        public Thing SourceThing { get; }

        public bool AffectsCell(IntVec3 pos);

        public void Notify_BuildingSpawned(Building building);
        public void Notify_BuildingDespawning(Building building);
        public void Notify_UpdateRadiation();
    }
}
