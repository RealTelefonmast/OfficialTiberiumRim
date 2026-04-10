using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore.Interfaces;
using TR.Designators;
using Verse;

namespace TR.SuperWeapon;

public interface ISuperWeapon
{
    void Notify_Fired();
}

public class SuperWeapon : IExposable, ICoolDownHolder, ISuperWeapon
{
    private TRBuildingPrototype _building;
    private Designator_Target resolvedDesignator;

    private int ticksUntilReady;

    public SuperWeapon()
    {
    }

    public SuperWeapon(TRBuildingPrototype building)
    {
        _building = building;
        //ticksUntilReady = Props.chargeTime.SecondsToTicks();
    }

    public virtual bool Active => _building.DestroyedOrNull() && IsPowered;

    public virtual bool CanFire => !CoolDownActive;

    public bool IsPowered => ((CompPowerTrader)_building.PowerComp).PowerOn;


    public SuperWeaponProperties Props => _building.def.superWeapon;

    private Designator ResolvedDesignator
    {
        get
        {
            if (resolvedDesignator == null)
            {
                resolvedDesignator = (Designator_Target)Activator.CreateInstance(Props.designator);
                resolvedDesignator.coolDown = this;
                resolvedDesignator.superWeapon = this;
            }

            return resolvedDesignator;
        }
    }

    public bool CoolDownActive => ticksUntilReady > 0;

    public float DisabledPct
    {
        get
        {
            float total = Props.chargeTime.SecondsToTicks();
            return ticksUntilReady / total;
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref _building, "building");
        Scribe_Values.Look(ref ticksUntilReady, "ticksUntilReady");
    }

    public void Notify_Fired()
    {
        if (CoolDownActive)
        {
            TRLog.Warning("Attempted to fire a super weapon while it was on cooldown.");
            return;
        }

        ticksUntilReady = Props.chargeTime.SecondsToTicks();
    }

    public virtual void Tick()
    {
        if (ticksUntilReady > 0) ticksUntilReady--;
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (DebugSettings.godMode)
            yield return new Command_Action
            {
                defaultLabel = "Reset Cooldown",
                action = delegate { ticksUntilReady = 0; }
            };

        yield return ResolvedDesignator;
    }
}