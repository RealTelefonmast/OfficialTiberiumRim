using System.Collections.Generic;
using Verse;

namespace TR.ThingData.Pawns.MechanicalPawns;

public class MechGarage : IThingHolder, IExposable
{
    private readonly int capacity;
    private ThingOwner container;

    public MechGarage(int capactiy)
    {
        capacity = capactiy;
        container = new ThingOwner<MechanicalPawn>(this, false, LookMode.Reference);
    }

    private bool CanAdd => capacity <= 0 || Container.Count < capacity;

    private bool CanAdd => capacity <= 0 || Container.Count < capacity;

    public ThingOwner Container
    {
        get => container;
        set => container = value;
    }

    public void ExposeData()
    {
        Scribe_Deep.Look(ref container, "container", this);
    }

    public IThingHolder ParentHolder => null;

    public IThingHolder ParentHolder => null;

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return Container;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return Container;
    }

    public bool TryPushToGarage(MechanicalPawn mech)
    {
        if (!CanAdd) return false;
        if (mech.Spawned)
            mech.DeSpawn();
        var i = Container.TryAddOrTransfer(mech, 1, false);
        return true;
    }

    public bool TryPullFromGarage(MechanicalPawn mech, out Thing resultingMech, IntVec3 toPos, Map map,
        ThingPlaceMode placeMode = ThingPlaceMode.Direct)
    {
        resultingMech = null;
        return Container.Contains(mech) && Container.TryDrop_NewTmp(mech, toPos, map, placeMode, out resultingMech);
    }
}

}