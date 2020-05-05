using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Veinhole : TiberiumProducer
    {
        private const int hubRadius = 70;
        private int ticksToHub = 0;
        private int ticksToEgg = 0;

        private int nutrients = 0;

        private List<Thing> boundHubs = new List<Thing>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            ResetEggTimer();
            ResetHubTimer();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToHub, "hubTicks");
            Scribe_Values.Look(ref ticksToEgg, "eggTicks");
            base.ExposeData();
        }

        public override void Tick()
        {
            if (ticksToHub == 0)
            {
                SpawnHub();

            }
            if (ticksToEgg == 0)
            {
                SpawnEgg();
            }
        }

        private void TryConsume(Pawn pawn)
        {

        }

        private void SpawnHub()
        {
            void Action(IntVec3 c)
            {
                if (c.SupportsTiberiumTerrain(Map)) Map.terrainGrid.SetTerrain(c, Ruleset.RandomTerrain());
            }

            TiberiumFloodInfo flood = new TiberiumFloodInfo(Map,null, Action);
            IntVec3 end = GenRadial.RadialCellsAround(Position, 56, false).RandomElement();
            flood.TryMakeConnection(out List<IntVec3> cells, Position, end);

            var hub = GenSpawn.Spawn(ThingDef.Named("VeinHub"), end, Map);
            boundHubs.Add(hub);
            ResetHubTimer();
        }

        public void RemoveHub(VeinHub hub)
        {
            if (boundHubs.Contains(hub))
                boundHubs.Remove(hub);
        }

        private void SpawnEgg()
        {
            var cell = FieldCells.RandomElement();

            GenSpawn.Spawn(ThingDef.Named("VeinEgg"), cell, Map);
            ResetEggTimer();
        }

        private void ResetHubTimer()
        {
            ticksToHub = (int)(GenDate.TicksPerDay * TRUtils.Range(3f, 7f));
        }

        private void ResetEggTimer()
        {
            ticksToEgg = (int)(GenDate.TicksPerDay * TRUtils.Range(1f, 3f));
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {

            foreach(Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            
            yield return new Command_Action{
                defaultLabel= "Spawn Hub",
                action = delegate
                {
                    SpawnHub();
                }
            };

        }
    }
}
