using System.Collections.Generic;
using Verse;

namespace TR;

public class PortalSpawner : TRBuilding, IThingHolder
{
    private ThingOwner container;
    private int lifeTicksLeft;
    private int ticksUntilDrop;
    private int ticksUntilNext = 750;

    protected virtual int ParticleTick => 10;

    public ThingOwner GetDirectlyHeldThings()
    {
        return container;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref container, "container", this, false, LookMode.Deep);
        Scribe_Values.Look(ref lifeTicksLeft, "ticksTilRelease");
    }

    public override void PostMake()
    {
        base.PostMake();
        container = new ThingOwner<Thing>(this, false);
    }

    public void PortalSetup(int lifeTicks, int ticksBetweenDrop = 750)
    {
        lifeTicksLeft = lifeTicks;
        ticksUntilNext = ticksBetweenDrop;
        ticksUntilDrop = ticksUntilNext;
    }

    public override void Tick()
    {
        base.Tick();
        if (this.IsHashIntervalTick(ParticleTick))
            DoParticleEffect();

        if (ticksUntilNext <= 0) ReleaseNext();
        if (lifeTicksLeft <= 0) DeSpawn();
        ticksUntilNext--;
        lifeTicksLeft--;
    }

    protected virtual void DoParticleEffect()
    {
    }

    public void Add(List<Thing> things)
    {
        foreach (var thing in things) Add(thing, thing.stackCount);
    }

    public void Add(Thing thing, int count = 1)
    {
        container.TryAdd(thing, count);
    }

    public void ReleaseNext()
    {
        ticksUntilNext = ticksUntilDrop;
        var dest = Position.RandomAdjacentCell8Way();
        foreach (var thing in container)
            if (thing != null)
            {
                container.TryDrop(thing, dest, Map, ThingPlaceMode.Direct, thing.stackCount, out var result);
                return;
            }
    }
}