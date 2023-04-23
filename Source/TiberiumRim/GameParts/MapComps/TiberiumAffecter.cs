using System.Collections.Generic;
using System.Linq;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumAffecter : MapInformation
    {
        private RadiationInfectionGrid hediffGrid;

        //Worker
        private IEnumerator<IntVec3> TileIterator;
        private bool dirtyIterator = false;

        public RadiationInfectionGrid HediffGrid => hediffGrid;
        public bool ShouldIterate => map.Tiberium().TiberiumInfo.TotalCount > 0;

        public TiberiumAffecter(Map map) : base(map)
        {
            hediffGrid = new RadiationInfectionGrid(map);
        }

        public override void ExposeDataExtra()
        {
            Scribe_Deep.Look(ref hediffGrid, "hediffGrid", map);
        }

        public override void Tick()
        {
            AffectCells();
        }

        private void AffectCells()
        {
            if (!ShouldIterate) return;

            //Setup Iterator
            if (TileIterator == null || dirtyIterator)
            {
                TileIterator = GetCurrentAffectedCells().GetEnumerator();
                dirtyIterator = false;
            }
            //Affect Objects
            if (TileIterator?.Current.IsValid ?? false)
            { 
                //TODO: Use CellSteadyEffects Instead -mark affected tiles
                //TODO: => MapWorldPatches.DoCellSteadyEffectsPatch
                var current = TileIterator.Current;
                TiberiumCrystal affecter = current.CellsAdjacent8Way().Select(c => c.GetTiberium(map)).FirstOrDefault();
                AffectPotentialObject(current, affecter);
            }
            if (!TileIterator.MoveNext())
                dirtyIterator = true;
        }

        private void AffectPotentialObject(IntVec3 cell, TiberiumCrystal affecter)
        {
            if (affecter == null) return;
            if (!affecter.def.DamagesThings) return;

            List<Thing> thingList = cell.GetThingList(map);
            for (var i = thingList.Count - 1; i >= 0; i--)
            {
                var thing = thingList[i];
                if (!thing.CanBeDamagedByTib(out float damageFactor)) continue;
                if (thing.def.useHitPoints)
                    thing.TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumDeterioration,damageFactor * TRandom.Range(affecter.def.tiberium.deteriorationDamage), 1));
                if (affecter.def.conversions.HasOutcomeFor(thing, out ThingConversion conversion) && Rand.Chance(conversion.chance))
                {
                    GenSpawn.Spawn(conversion.GetOutcome(), thing.Position, map);
                    if (!thing.DestroyedOrNull())
                        thing.DeSpawn();
                }
            }
        }

        private IEnumerable<IntVec3> GetCurrentAffectedCells()
        {
            var mapComp = map.Tiberium();
            var tibInfo = mapComp.TiberiumInfo;
            var tibGrid = tibInfo.TiberiumGrid;
            return tibGrid.AffectedCells.ActiveCells;
        }

        public void Notify_TibChanged()
        {
            dirtyIterator = true;
        }

        public void AddRadiation(IntVec3 pos, float pctValue)
        {
            hediffGrid.AddRadiation(pos, pctValue);
        }

        public void AddInfection(IntVec3 pos, float pctValue)
        {
            hediffGrid.AddInfection(pos, pctValue);
        }
    }
}
