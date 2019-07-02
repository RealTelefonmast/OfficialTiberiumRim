using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class TurretGun : FXThing, IVerbOwner
    {
        //The TurretGun is a component of any complex turret, it will act individually, or as part of the whole parent object
        public new TRThingDef def;
        public VerbTracker verbTracker;
        public Building_TRTurret parent;
        public TurretProperties turret;

        private int warmupTicksLeft;
        private int cooldownTicksLeft;
        private int rotation = 0;
        private LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        public Pawn Holder => CurrentVerb.CasterPawn;
        public List<Verb> AllVerbs => verbTracker.AllVerbs;
        public Verb PrimaryVerb => verbTracker.PrimaryVerb;
        public Verb SecondaryVerb => AllVerbs[1];
        public VerbTracker VerbTracker => verbTracker;
        public List<VerbProperties> VerbProperties => def.Verbs;
        public List<Tool> Tools => def.tools;
        public Thing ConstantCaster => this;
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Weapon;

        public Verb CurrentVerb
        {
            get
            {
                return PrimaryVerb;
            }
        }

        public LocalTargetInfo CurrentTarget => currentTargetInt;

        //Make SetUp - Define initial values and workers
        public override void PostMake()
        {
            base.PostMake();
            this.def = (TRThingDef)base.def;
            if (def.tickerType != TickerType.Never)
            {
                Log.Error(this + " should have TickerType 'Never'");
            }
            verbTracker = new VerbTracker(this);
        }

        public int TurretRotation
        {
            get => rotation;
            set
            {
                if(value > 360)
                {
                    rotation = value - 360;
                }
                if(value < 0)
                {
                    rotation = value + 360;
                }
                rotation = value;
            }
        }

        public bool Available
        {
            get
            {
                return true;
            }
        }

        public void TurretGunTick()
        {
            if (!CurrentTarget.IsValid)
            {

            }
        }

        public void ResetTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            warmupTicksLeft = 0;
        }

        //Drawing The Turret
        public override Vector3 DrawPos => parent.DrawPos + turret.drawOffset;

        public Vector3 ProjectileOrigin
        {
            get
            {
                Vector3 drawPos = DrawPos;
                return drawPos;
            }
        }

        public override void Draw()
        {
            base.Draw(); 
        }

        public string UniqueVerbOwnerID()
        {
            return "TurretGun_" + this.parent.ThingID;
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return false;
        }
    }
}
