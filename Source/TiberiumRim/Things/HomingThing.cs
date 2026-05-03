using UnityEngine;
using Verse;

namespace TiberiumRim;

public class HomingThingDef : TRThingDef
{
    public bool destroyOnArrival = false;
    public float liveTime = 10;
    public FloatRange speed;
}

public class HomingThing : ThingWithComps
{
    public new HomingThingDef def;
    private Vector3 exactPos;
    private float speed = 1f;
    public TargetInfo Target;

    private int ticksToLive;

    public override Vector3 DrawPos => exactPos;

    private bool ShouldDestroy => ticksToLive <= 0 || (def.destroyOnArrival && ActualPosition == Target.Cell);

    private IntVec3 ActualPosition => exactPos.ToIntVec3();

    public Vector3 Velocity => (Target.CenterVector3 - exactPos).normalized * speed;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        def = (HomingThingDef)base.def;
        speed = TRUtils.Range(def.speed);
        exactPos = Position.ToVector3();
        ticksToLive = def.liveTime.SecondsToTicks();
        base.SpawnSetup(map, respawningAfterLoad);
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
        Target = target;
    }

    public override void Tick()
    {
        base.Tick();
        exactPos = exactPos + Velocity * 0.0166666675f;

        if (ticksToLive > 0)
            ticksToLive--;

        if (ShouldDestroy)
            Destroy();
    }
}