using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;

namespace TiberiumRim
{
    public interface ITurretHolder
    {
        List<LocalTargetInfo> CurrentTargets { get; }
        LocalTargetInfo FocusedTarget { get; }

        bool PlayerControlled { get; }
        bool CanSetForcedTarget { get; }

        bool CanToggleHoldFire { get; }
        bool HoldingFire { get; }

        bool HasTarget(Thing target);
        void AddTarget(LocalTargetInfo target);
        void RemoveTargets();
    }

    public class Building_TRTurret : Building_Turret, IFXObject, ITurretHolder
    {
        public TRThingDef def;
        public List<TurretGun> turrets = new List<TurretGun>();
        private List<LocalTargetInfo> targets = new List<LocalTargetInfo>();
        private LocalTargetInfo focusedTarget = LocalTargetInfo.Invalid;

        private CompMannable mannableComp;
        private bool holdFire;

        public override LocalTargetInfo CurrentTarget => null;
        public LocalTargetInfo FocusedTarget => focusedTarget;
        public List<LocalTargetInfo> CurrentTargets => targets;
        public override Verb AttackVerb => null;
        public TurretGun MainGun => turrets.First();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TRThingDef)base.def;
            foreach (TurretProperties props in def.turret.turrets)
            {
                var turret = new TurretGun(props, this);
                turrets.Add(turret);
                turret.Setup();
            }
        }

        public override void Tick()
        {
            base.Tick();
            foreach (TurretGun gun in turrets)
            {
                gun.TurretTick();
            }
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            
        }

        public void CommandHoldFire()
        {
            holdFire = !holdFire;
            if (holdFire)
            {
                foreach (TurretGun gun in turrets)
                {
                    gun.ResetForcedTarget();
                }
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

        public bool MannedByColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }
        private bool MannedByNonColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }
        public bool PlayerControlled
        {
            get
            {
                return (this.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        public bool HoldingFire => holdFire;
        public bool CanSetForcedTarget => PlayerControlled && (def.turret.canForceTarget || mannableComp != null);
        public bool CanToggleHoldFire => this.PlayerControlled;

        public virtual ExtendedGraphicData ExtraData => def.extraData;
        // Base connection, base glow, turret glow
        public virtual Vector3[] DrawPositions => new Vector3[3] { base.DrawPos, base.DrawPos, base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[3] { Color.white, Color.white, Color.white };
        public virtual float[] OpacityFloats => new float[3] { 1f ,1f, 1f};
        public virtual float?[] RotationOverrides => new float?[3] { null, null, MainGun.TurretRotation };
        public virtual bool[] DrawBools => new bool[3] { true, true, true };
        public virtual bool ShouldDoEffecters => true;

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
                    action = delegate (Thing thing)
                    {
                        Targeter targeter = Find.Targeter;
                        foreach (TurretGun turret in turrets)
                        {
                            turret.SetForcedTarget(thing);
                        }
                    }
                };
            }
            if (this.CanToggleHoldFire)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                    hotKey = KeyBindingDefOf.Misc6,
                    toggleAction = new Action(CommandHoldFire),
                    isActive = (() => holdFire)
                };
            }
            if (DebugSettings.godMode)
            {

            }
            yield break;
        }
    }
}
