using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface ITurretHolder
    {
        List<LocalTargetInfo> CurrentTargets { get; }
        LocalTargetInfo CurrentTarget { get; }
        ThingWithComps Caster { get; }
        Thing Parent { get; }
        CompRefuelable RefuelComp { get; }
        CompPowerTrader PowerComp { get; }
        CompMannable MannableComp { get; }
        StunHandler Stunner { get; }

        bool MannedByColonist { get; }
        bool PlayerControlled { get; }
        bool CanSetForcedTarget { get; }

        bool CanToggleHoldFire { get; }
        bool HoldingFire { get; }
        bool IsReady { get; }

        bool HasTarget(Thing target);
        void AddTarget(LocalTargetInfo target);
        void RemoveTargets();
        void Notify_ProjectileFired();
    }

    public class Building_TRTurret : Building_Turret, IFXObject, ITurretHolder
    {
        public new TRThingDef def;
        public List<TurretGun> turrets = new List<TurretGun>();

        private bool holdFire;

        public ThingWithComps Caster => this;
        public Thing Parent => this;

        //Main Target
        protected List<LocalTargetInfo> targets = new List<LocalTargetInfo>();
        public override LocalTargetInfo CurrentTarget => forcedTarget;
        public List<LocalTargetInfo> CurrentTargets => targets;
        public override Verb AttackVerb => MainGun.AttackVerb;
        public TurretGun MainGun => turrets.Any() ? turrets.First() : null;

        public virtual CompRefuelable RefuelComp => GetComp<CompRefuelable>();
        public new virtual CompPowerTrader PowerComp => GetComp<CompPowerTrader>();
        public virtual CompMannable MannableComp => GetComp<CompMannable>();
        public virtual StunHandler Stunner => stunner;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            SetupTurrets();
        }

        public virtual void SetupTurrets()
        {
            if (def.turret?.turrets == null) return;
            foreach (TurretProperties props in def.turret.turrets)
            {
                AddTurret(props);
            }
        }

        protected void AddTurret(TurretProperties props)
        {
            var turret = (TurretGun)Activator.CreateInstance(props.turretGunClass);
            turrets.Add(turret);
            turret.Setup(props, this);
        }

        public override void Tick()
        {
            base.Tick();
            foreach (TurretGun turret in turrets)
            {
                turret.TurretTick(IsReady);
            }
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            //Already have a target, but ordered target invalid
            if (!targ.IsValid && CurrentTarget.IsValid)
            {
                ResetForcedTarget();
                return;
            }
            if ((targ.Cell - Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if ((targ.Cell - Position).LengthHorizontal > AttackVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                return;
            }
            //Set forced target for all turrets
            if (forcedTarget != targ)
            {
                forcedTarget = targ;
                turrets.ForEach(t => t.OrderAttack(targ));
            }
            if (holdFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, false);
            }
        }

        protected virtual void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            turrets.ForEach(t => t.ResetForcedTarget());
        }

        public void CommandHoldFire()
        {
            holdFire = !holdFire;
            if (holdFire)
            {
                ResetForcedTarget();
            }
        }

        public override void Draw()
        {
            base.Draw();
            foreach(TurretGun gun in turrets)
            {
                gun.Draw();
            }
        }

        public bool HasTarget(Thing target)
        {
           return targets.Contains(target);
        }

        public void AddTarget(LocalTargetInfo target)
        {
            if (target.IsValid)
            {
                targets.Add(target);
            }
        }
        public void RemoveTargets()
        {
            foreach(LocalTargetInfo target in targets)
            {
                if (!target.IsValid)
                    targets.Remove(target);
            }
        }

        public virtual void Notify_ProjectileFired()
        {

        }

        public bool MannedByColonist
        {
            get
            {
                return MannableComp != null && MannableComp.ManningPawn != null && MannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }
        private bool MannedByNonColonist
        {
            get
            {
                return MannableComp != null && MannableComp.ManningPawn != null && MannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }
        public bool PlayerControlled
        {
            get
            {
                return (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;
            }
        }

        public virtual bool IsReady => Spawned && (PowerComp == null || PowerComp.PowerOn) && (MannableComp == null || MannableComp.MannedNow);
        public virtual bool HoldingFire => holdFire;
        public virtual bool CanSetForcedTarget => PlayerControlled && (def.turret.canForceTarget || MannableComp != null);
        public virtual bool CanToggleHoldFire => PlayerControlled;

        public virtual ExtendedGraphicData ExtraData => def.extraData;
        // Base connection, base glow, turret glow
        public virtual Vector3[] DrawPositions => new Vector3[3] { base.DrawPos, base.DrawPos, base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[3] { Color.white, Color.white, Color.white };
        public virtual float[] OpacityFloats => new float[3] { 1f ,1f, 1f};
        public virtual float?[] RotationOverrides => new float?[3] { null, null, MainGun?.TurretRotation };
        public float?[] AnimationSpeeds => null;
        public virtual bool[] DrawBools => new bool[3] { true, true, true };
        public virtual Action<FXGraphic>[] Actions => null;

        public virtual Vector2? TextureOffset => null;
        public virtual Vector2? TextureScale => null;
        public virtual bool ShouldDoEffecters => true;
        public virtual CompPower ForcedPowerComp => null;

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                sb.AppendLine(inspectString);
            }
            sb.AppendLine("Active turrets: " + turrets.Count);
            if (!turrets.Any()) return sb.ToString().TrimEndNewlines();

            sb.AppendLine("-- Main Turret --");
            if (AttackVerb.verbProps.minRange > 0f)
            {
                sb.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
            }
            if (Spawned && MainGun.NeedsRoof && Position.Roofed(Map))
            {
                sb.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (Spawned && MainGun.burstCooldownTicksLeft > 0 && MainGun.BurstCooldownTime() > 5f)
            {
                sb.AppendLine("CanFireIn".Translate() + ": " + MainGun.burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }
            CompChangeableProjectile compChangeableProjectile = MainGun.Gun.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile != null)
            {
                if (compChangeableProjectile.Loaded)
                    sb.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
                else
                    sb.AppendLine("ShellNotLoaded".Translate());
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            /*
            IEnumerator<Gizmo> enumerator = null;
            CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile != null)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "CommandExtractShell".Translate();
                command_Action.defaultDesc = "CommandExtractShellDesc".Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get("Rimatomics/Things/Resources/sabot/sabot_c", true);
                command_Action.alsoClickIfOtherInGroupClicked = false;
                command_Action.action = new Action(this.dumpShells);
                if (compChangeableProjectile.Projectile == null)
                {
                    command_Action.Disable("NoSabotToExtract".Translate());
                }
                yield return command_Action;
            }
            */
            if (CanSetForcedTarget)
            {
                yield return new Command_Target
                {
                    defaultLabel = "CommandSetForceAttackTarget".Translate(),
                    defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true),
                    targetingParams = TargetingParameters.ForAttackAny(),
                    action = OrderAttack
                };
            }
            if (this.CanToggleHoldFire)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                    toggleAction = new Action(CommandHoldFire),
                    isActive = (() => holdFire)
                };
            }
            if (DebugSettings.godMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Rotate 30°",
                    action = delegate
                    {
                        MainGun.top.CurRotation += 30;
                    }
                };
            }
            yield break;
        }
    }
}
