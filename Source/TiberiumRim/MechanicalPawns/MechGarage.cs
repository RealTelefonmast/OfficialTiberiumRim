using System.Collections.Generic;
using Verse;

namespace TR
{
    public class MechGarage : IThingHolder, IExposable
    {
        private ThingOwner container;
        private readonly int capacity;

        public IThingHolder ParentHolder => null;

        private bool CanAdd => capacity <= 0 || Container.Count < capacity;

        public MechGarage(int capactiy)
        {
            this.capacity = capactiy;
            container = new ThingOwner<MechanicalPawn>(this, false, LookMode.Reference);
        }

        public ThingOwner Container
        {
            get => container;
            set => container = value;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref container, "container", this);
        }

        public bool TryPushToGarage(MechanicalPawn mech)
        {
            if (!CanAdd) return false;
            if(mech.Spawned)
                mech.DeSpawn();
            int i = Container.TryAddOrTransfer(mech, 1, false);
            return true;
        }

        public bool TryPullFromGarage(MechanicalPawn mech, out Thing resultingMech, IntVec3 toPos, Map map, ThingPlaceMode placeMode = ThingPlaceMode.Direct)
        {
            resultingMech = null;
            return Container.Contains(mech) && Container.TryDrop(mech, toPos, map, placeMode, out resultingMech);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            return;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return Container;
        }
    }
}
