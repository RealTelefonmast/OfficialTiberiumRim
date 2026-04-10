using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public interface IHarvestPreferenceSettable
    {
        Map Map { get; }

        TiberiumCrystalDef GetTiberiumDefToPrefer();

        void SetTiberiumDefToPrefer(TiberiumCrystalDef tibDef);
    }
}
