using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using RimWorld;

namespace TiberiumRim
{
    public class Building_TiberiumGeyser : Building
    {
        public Building harvester;

        private int radius = 12;

        public TiberiumType tiberiumType;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.tiberiumType = Rand.Element(TiberiumType.Green, TiberiumType.Blue, TiberiumType.Red);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<TiberiumType>(ref tiberiumType, "tiberiumType");
        }

        public override void TickLong()
        {
            if (this.harvester == null)
            {
                int num = GenRadial.NumCellsInRadius(this.radius);

                for (int i = 0; i < num; i++)
                {
                    IntVec3 positionToCheck = this.Position + GenRadial.RadialPattern[i];
                    this.GasAttack(positionToCheck);
                }

            }
        }

        public void GasAttack(IntVec3 c)
        {
            if (c.InBounds(Map))
            {
                List<Thing> thinglist = c.GetThingList(this.Map);
                for (int i = 0; i < thinglist.Count; i++)
                {
                    if (thinglist[i] is Pawn)
                    {
                        ThingDef crack = ThingDef.Named("TiberiumCrack");
                        if (c.GetFirstThing(Map, crack) == null)
                        {
                            GenSpawn.Spawn(crack, c, Map);
                        }
                    }
                }
            }
        }
    }

    public class PlaceWorker_OnTiberiumGeyser : PlaceWorker
    {
        ThingDef geyser = ThingDef.Named("TiberiumGeyser");
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            Thing thing = map.thingGrid.ThingAt(loc, geyser);
            if (thing == null || thing.Position != loc)
            {
                return "MustPlaceOnTiberiumGeyser".Translate();
            }
            return true;
        }

        public override bool ForceAllowPlaceOver(BuildableDef otherDef)
        {
            return otherDef == geyser;
        }
    }

}
