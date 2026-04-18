using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

//using Multiplayer.API;

namespace TeleCore.Unsorted;

public class TurretGunSet : IExposable
{
    private readonly ITurretHolder parent;

    public TurretGunSet(TurretDefExtension holderProps, ITurretHolder parent)
    {
        TLog.Message($"Holder[{parent?.HolderThing}]: {holderProps != null} | {holderProps?.turrets?.Count}");
        this.parent = parent;
        Turrets = new List<TurretGun>(holderProps.turrets.Count);
        var set = this;
        for (var i = 0; i < holderProps.turrets.Count; i++)
        {
            var props = holderProps.turrets[i];
            var turret = (TurretGun) Activator.CreateInstance(props.turretGunClass);
            turret.Setup(props, i, this, parent);
            Turrets.Add(turret);
            MainGun ??= turret;
        }
    }

    public TurretGunSet(IReadOnlyList<TurretProperties> turretProps, ITurretHolder parent)
    {
        this.parent = parent;
        Turrets = new List<TurretGun>(turretProps.Count);
        var set = this;
        for (var i = 0; i < turretProps.Count; i++)
        {
            var props = turretProps[i];
            var turret = (TurretGun) Activator.CreateInstance(props.turretGunClass, new {props, i, set, parent});
            Turrets.Add(turret);
            MainGun ??= turret;
        }
    }

    public List<LocalTargetInfo> KnownTargets { get; } = new();

    //
    public List<TurretGun> Turrets { get; }

    public TurretGun MainGun { get; }

    public Verb AttackVerb => MainGun.AttackVerb;

    //
    public bool HoldingFire { get; private set; }

    public void ExposeData()
    {
    }

    public void TickTurrets()
    {
        foreach (var turret in Turrets)
        {
            turret.TickGun();
        }
    }

    //
    public void TryOrderAttack(LocalTargetInfo targ)
    {
        foreach (var turretGun in Turrets) 
            turretGun.TryOrderAttack(targ);
    }

    //[SyncMethod]
    public void CommandHoldFire()
    {
        HoldingFire = !HoldingFire;
        if (HoldingFire) ResetOrderedAttack();
    }

    public void ResetOrderedAttack()
    {
        Turrets.ForEach(t => t.ResetForcedTarget());
    }

    //
    public bool HasTarget(LocalTargetInfo target)
    {
        return KnownTargets.Contains(target);
    }

    public void Notify_NewTarget(LocalTargetInfo currentTarget)
    {
        if (HasTarget(currentTarget)) return;
        KnownTargets.Add(currentTarget);
    }

    public void Notify_LostTarget(LocalTargetInfo target)
    {
        KnownTargets.Remove(target);
    }

    //Drawing
    public void Draw(Vector3 drawLoc)
    {
        foreach (var gun in Turrets) 
            gun.Draw(drawLoc);
    }

    public IEnumerable<Gizmo> TurretGizmos()
    {
        yield return new Command_Toggle
        {
            defaultLabel = "CommandHoldFire".Translate(),
            defaultDesc = "CommandHoldFireDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
            toggleAction = CommandHoldFire,
            isActive = () => HoldingFire
        };
    }

    static StringBuilder sb = new StringBuilder();
    public string InspectString()
    {
        sb.Clear();

        if (DebugSettings.godMode)
        {
            sb.AppendLine($"Active turrets: {Turrets.Count}");
            sb.AppendLine($"PlayerControlled: {this.parent.PlayerControlled}");
            sb.AppendLine($"CanSetForcedTarget: {MainGun.CanSetForcedTarget}");
        }

        if (!Enumerable.Any(Turrets)) return sb.ToString().TrimEndNewlines();

        //sb.AppendLine("-- Main Turret --");
        if (AttackVerb.verbProps.minRange > 0f)
            sb.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
        var parent = this.parent.HolderThing;
        if (parent.Spawned && (MainGun.NeedsRoofless && parent.Position.Roofed(parent.Map)))
            sb.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
        else if (parent.Spawned && MainGun is { BurstCoolDownTicksLeft: > 0, BurstCooldownTime: > 5f })
            sb.AppendLine("CanFireIn".Translate() + ": " + MainGun.BurstCoolDownTicksLeft.ToStringSecondsFromTicks());
        var compChangeableProjectile = MainGun.Gun.TryGetComp<CompChangeableProjectile>();
        if (compChangeableProjectile != null)
        {
            if (compChangeableProjectile.Loaded)
                sb.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap,
                    compChangeableProjectile.LoadedShell));
            else
                sb.AppendLine("ShellNotLoaded".Translate());
        }

        return sb.ToString();
    }

    public void DoAttackNow(LocalTargetInfo targ, int turretIndex)
    {
        if(turretIndex >= Turrets.Count) return;
        if (turretIndex >= 0)
        {
            Turrets[turretIndex].ForceOrderAttack(targ);
            return;
        }
        
        foreach (var turret in Turrets)
        {
            turret.ForceOrderAttack(targ);
        }
    }

    public void ClearAttack()
    {
        foreach (var turret in Turrets)
        {
            turret.ResetCurrentTarget();
        }
    }
}