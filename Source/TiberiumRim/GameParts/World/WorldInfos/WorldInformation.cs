using RimWorld;
using RimWorld.Planet;
using TR.ThingData;
using Verse;

namespace TR.GameParts.WorldInfos;

public abstract class WorldInformation : IExposable
{
    private World world;

    public WorldInformation(World world)
    {
        this.world = world;
    }

    public virtual void ExposeData()
    {
    }

    public virtual void Setup()
    {
    }

    public virtual void InfoTick()
    {
    }

    public virtual void Notify_EventHappened(string tag, IIncidentTarget location)
    {
    }

    public virtual void Notify_RegisterWorldObject(GlobalTargetInfo worldObjectOrThing)
    {
    }

    public virtual void Notify_BuildingSpawned(TRBuildingPrototype building)
    {
    }
}