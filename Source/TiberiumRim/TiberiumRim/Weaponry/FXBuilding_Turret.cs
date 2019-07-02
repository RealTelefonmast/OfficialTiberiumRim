using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class FXBuilding_Turret : Building_TurretGun, IFXObject
    {
        public LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;

        public ExtendedGraphicData ExtraData => (base.def as FXThingDef).extraData;
        public CompFX FXComp => this.GetComp<CompFX>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override LocalTargetInfo CurrentTarget => currentTarget;
        public override Verb AttackVerb => GunCompEq.PrimaryVerb;

        public bool WarmingUp => burstWarmupTicksLeft > 0;


        public virtual Vector3[] DrawPositions => new Vector3[1] { base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[1] { Color.white };
        public virtual float[] OpacityFloats => new float[1] { 1f };
        public virtual float?[] RotationOverrides => new float?[1] { null };
        public virtual bool[] DrawBools => new bool[1] { true };
        public virtual bool ShouldDoEffecters => true;
    }

    public class FXTurretTop
    {

    }
}
