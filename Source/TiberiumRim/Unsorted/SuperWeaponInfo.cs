using System;
using RimWorld;
using Verse;

namespace TiberiumRim;

public class SuperWeaponProperties
{
    public float chargeTime;
    public Type designator;

    private Designator resolvedDesignator;
    public Type worker = typeof(SuperWeaponInfo);

    public Designator ResolvedDesignator
    {
        get
        {
            if (resolvedDesignator == null) resolvedDesignator = (Designator)Activator.CreateInstance(designator);
            return resolvedDesignator;
        }
    }
}

public class SuperWeaponInfo
{
    public TRBuilding building;
    public int ticksUntilReady;

    public virtual bool Active => building.DestroyedOrNull() && IsPowered;

    public virtual bool CanFire => ticksUntilReady <= 0;

    public bool IsPowered => ((CompPowerTrader)building.PowerComp).PowerOn;
}