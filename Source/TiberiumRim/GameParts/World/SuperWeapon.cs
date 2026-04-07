using System;
using RimWorld;
using Verse;

namespace TR.GameParts;

public class SuperWeapon : IExposable
{
    public TRBuilding building;
    public int ticksUntilReady;

    public virtual bool Active => building.DestroyedOrNull() && IsPowered;

    public virtual bool CanFire => ticksUntilReady <= 0;

    public bool IsPowered => ((CompPowerTrader)building.PowerComp).PowerOn;

    public void ExposeData()
    {
        Scribe_References.Look(ref building, "building");
        Scribe_Values.Look(ref ticksUntilReady, "ticksUntilReady");
    }
}

public class SuperWeaponProperties
{
    public float chargeTime;
    public Type designator;

    private Designator resolvedDesignator;
    public Type worker = typeof(SuperWeapon);

    public Designator ResolvedDesignator
    {
        get
        {
            if (resolvedDesignator == null) resolvedDesignator = (Designator)Activator.CreateInstance(designator);
            return resolvedDesignator;
        }
    }
}