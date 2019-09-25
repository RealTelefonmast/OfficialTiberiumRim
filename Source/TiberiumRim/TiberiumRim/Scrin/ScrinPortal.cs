using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class ScrinPortal : TRBuilding, IThingHolder
    {
        private ThingOwner container;
        private int ticksUntilRelease;
        private int ticksUntilNext = 750;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref container, "container", new object[]
            {
                this,
                false,
                LookMode.Deep
            });
            Scribe_Values.Look(ref ticksUntilRelease, "ticksTilRelease");
        }

        public override void PostMake()
        {
            base.PostMake();
            container = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public void PortalSetup(int waitTicks, int ticksBetweenDrop = 750)
        {
            ticksUntilRelease = waitTicks;
            ticksBetweenDrop = 750;
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksUntilRelease <= 0)
            {
                if (container.NullOrEmpty())
                {
                    this.DeSpawn();
                }
                if (ticksUntilNext <= 0)
                {
                    ReleaseNext();
                    ticksUntilNext = 750;
                }
                ticksUntilNext--;
            }
            ticksUntilRelease--;
        }

        public void Add(List<Thing> things)
        {
            foreach (var thing in things)
            {
                Add(thing, thing.stackCount);
            }
        }

        public void Add(Thing thing, int count = 1)
        {
            container.TryAdd(thing, count);
        }

        public void ReleaseNext()
        {
            IntVec3 dest = Position.RandomAdjacentCell8Way();
            foreach (var thing in container)
            {
                if (thing != null)
                {
                    container.TryDrop(thing, dest, Map, ThingPlaceMode.Direct, thing.stackCount, out Thing result);
                    return;
                }
            }
        }


        public ThingOwner GetDirectlyHeldThings()
        {
            return this.container;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
    }
}
