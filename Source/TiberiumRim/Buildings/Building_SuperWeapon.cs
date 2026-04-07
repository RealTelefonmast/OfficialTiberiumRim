using System.Collections.Generic;
using TR.ThingData;
using Verse;

namespace TR.Buildings;

public class Building_SuperWeapon : TRBuildingPrototype
{
    private SuperWeapon _superWeapon;

    internal void RegisterSWep(SuperWeapon wepWorker)
    {
        _superWeapon = wepWorker;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (_superWeapon != null)
            foreach (var gizmo in _superWeapon.GetGizmos())
                yield return gizmo;
    }
}