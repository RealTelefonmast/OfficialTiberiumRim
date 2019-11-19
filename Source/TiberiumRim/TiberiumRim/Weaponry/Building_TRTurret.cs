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
    public class Building_TRTurret : Building_Turret, IFXObject
    {
        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
        public new TRThingDef def;

        protected int burstCooldownTicksLeft;
        protected int burstWarmupTicksLeft;
        protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;
        private bool holdFire;
        public Thing gun;
        protected TurretTop top;
        protected CompPowerTrader powerComp;
        protected CompMannable mannableComp;
        private const int TryStartShootSomethingIntervalTicks = 10;

        public List<TurretGun> Turrets = new List<TurretGun>();

        public virtual Vector3[] DrawPositions => new Vector3[1] { base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[1] { Color.white };
        public virtual float[] OpacityFloats => new float[1] { 1f };
        public virtual float?[] RotationOverrides => new float?[1] { null };
        public virtual bool[] DrawBools => new bool[1] { true };
        public virtual bool ShouldDoEffecters => true;
        public ExtendedGraphicData ExtraData => (base.def as FXThingDef).extraData;
        public override LocalTargetInfo CurrentTarget => currentTargetInt;
        public override Verb AttackVerb => throw new NotImplementedException();
       
        public override void PostMake()
        {
            base.PostMake();
            foreach (ThingDef turret in def.turret.turrets)
            {
                Turrets.Add((TurretGun)ThingMaker.MakeThing(turret, this.Stuff));
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (TRThingDef)base.def;
        }

        public override void Tick()
        {
            base.Tick();
            foreach (TurretGun turret in Turrets)
            {
                turret.TurretGunTick();
            }
        }

        public bool CanForceTarget
        {
            get
            {
                return true;
            }
        }

        public bool HoldingFire
        {
            get
            {
                return false;
            }
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            throw new NotImplementedException();
        }

        public void ResetTargets()
        {
            Turrets.ForEach(t => t.ResetTarget());
        }

        public override void Draw()
        {
            base.Draw();
        }

        public void DrawMarkedForDeath(LocalTargetInfo target)
        {
            Material mat = MaterialPool.MatFrom(TRMats.MarkedForDeath, ShaderDatabase.MetaOverlay, Color.white);
            float num = (Time.realtimeSinceStartup + 397f * (float)(target.Thing.thingIDNumber % 571)) * 4f;
            float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
            var c = target.CenterVector3 + new Vector3(0, 0, 1.15f);
            Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z), Quaternion.identity, material, 0);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (CanForceTarget)
            {
                Command_VerbTarget attack = new Command_VerbTarget();
                attack.defaultLabel = "CommandSetForceAttackTarget".Translate();
                attack.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
                attack.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
                attack.verb = this.AttackVerb;
                attack.hotKey = KeyBindingDefOf.Misc4;
                /*
                if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
                {
                    attack.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
                }
                */
                yield return attack;
            }
            if (CurrentTarget.IsValid)
            {
                Command_Action stop = new Command_Action();
                stop.defaultLabel = "CommandStopForceAttack".Translate();
                stop.defaultDesc = "CommandStopForceAttackDesc".Translate();
                stop.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
                stop.action = delegate ()
                {
                //this.ResetForcedTarget();
                   // SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                };
                if (!this.forcedTarget.IsValid)
                {
                    stop.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                }
                stop.hotKey = KeyBindingDefOf.Misc5;
                yield return stop;
            }
        }
    }
}
