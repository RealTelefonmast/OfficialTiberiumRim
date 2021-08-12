using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;


namespace TiberiumRim
{
    public class HomingThingDef : TRThingDef
    {
        public FloatRange speed;
        public bool destroyOnArrival = false;
        public float liveTime = 10;
    }

    public class HomingThing : ThingWithComps
    {
        public new HomingThingDef def;
        public TargetInfo Target;

        private int ticksToLive;
        private float speed = 1f;
        private Vector3 exactPos;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            speed = TRUtils.Range(def.speed);
            exactPos = Position.ToVector3();
            ticksToLive = def.liveTime.SecondsToTicks();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            base.PostMake();
            def = (HomingThingDef)base.def;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Target, "Target");
            Scribe_Values.Look(ref exactPos, "exactPos");
            Scribe_Values.Look(ref ticksToLive, "ticksToLive");
            base.ExposeData();
        }

        public void SetTarget(TargetInfo target)
        {
            this.Target = target;
        }

        public override void Tick()
        {
            base.Tick();
            exactPos = this.exactPos + Velocity * 0.0166666675f;

            if (ticksToLive > 0)
                ticksToLive--;

            if(ShouldDestroy)
                this.Destroy();
        }

        public override Vector3 DrawPos => exactPos;

        private bool ShouldDestroy => ticksToLive <= 0 || (def.destroyOnArrival && ActualPosition == Target.Cell);

        private IntVec3 ActualPosition => exactPos.ToIntVec3();

        public Vector3 Velocity => (Target.CenterVector3 - exactPos).normalized * speed;
    }
}
