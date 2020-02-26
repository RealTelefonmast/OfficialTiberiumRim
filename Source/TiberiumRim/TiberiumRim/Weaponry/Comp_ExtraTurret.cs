using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_ExtraTurret : ThingComp, ITurretHolder
    {
        public List<TurretGun> turrets = new List<TurretGun>();

        public List<LocalTargetInfo> CurrentTargets => throw new NotImplementedException();
        public LocalTargetInfo CurrentTarget => throw new NotImplementedException();
        public bool PlayerControlled => throw new NotImplementedException();
        public bool CanSetForcedTarget => throw new NotImplementedException();
        public bool CanToggleHoldFire => throw new NotImplementedException();
        public bool HoldingFire => throw new NotImplementedException();
        public bool MannedByColonist => false;

        public CompRefuelable RefuelComp => throw new NotImplementedException();
        public CompPowerTrader PowerComp => throw new NotImplementedException();
        public CompMannable MannableComp => null;
        public StunHandler Stunner => throw new NotImplementedException();

        public bool IsReady => throw new NotImplementedException();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach(TurretProperties props in (props as CompProperties_ExtraTurret).turrets)
            {
                var turret = (TurretGun)Activator.CreateInstance(props.turretGunClass);
                turrets.Add(turret);
                turret.Setup(props, parent);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            foreach (TurretGun turret in turrets)
            {
                turret.TurretTick(false);
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            foreach (TurretGun turret in turrets)
            {
                turret.Draw();
            }
        }

        public bool HasTarget(Thing target)
        {
            throw new NotImplementedException();
        }

        public void AddTarget(LocalTargetInfo target)
        {
            throw new NotImplementedException();
        }

        public void RemoveTargets()
        {
            throw new NotImplementedException();
        }

        public void Notify_ProjectileFired()
        {
            throw new NotImplementedException();
        }
    }

    public class CompProperties_ExtraTurret : CompProperties
    {
        public List<TurretProperties> turrets;

        public CompProperties_ExtraTurret()
        {
            compClass = typeof(Comp_ExtraTurret);
        }
    }
}
