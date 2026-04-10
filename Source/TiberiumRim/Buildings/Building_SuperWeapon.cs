using System.Collections.Generic;
using Verse;

namespace TR;

public class Building_SuperWeapon : TRBuildingPrototype
{
    private SuperWeapon.SuperWeapon _superWeapon;

    internal void RegisterSWep(SuperWeapon.SuperWeapon wepWorker)
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