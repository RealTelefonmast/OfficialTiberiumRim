using System;
using TR.ThingData;
using Verse;

namespace TR;

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