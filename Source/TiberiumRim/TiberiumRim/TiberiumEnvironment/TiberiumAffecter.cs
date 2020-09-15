using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TiberiumAffecter : MapInformation
    {
        private TiberiumHediffGrid hediffGrid;

        private IEnumerator<IntVec3> TileIterator;
        private bool dirtyIterator = false;

        public TiberiumHediffGrid HediffGrid => hediffGrid;
        public bool ShouldIterate => map.Tiberium().TiberiumInfo.TotalCount > 0;

        public TiberiumAffecter(Map map) : base(map)
        {
            hediffGrid = new TiberiumHediffGrid(map);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref hediffGrid, "hediffGrid", map);
        }

        public void Tick()
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
                    thing.TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumDeterioration,damageFactor * TRUtils.Range(affecter.def.props.deteriorationDamage), 1));
                if (affecter.def.conversions.HasOutcomeFor(thing, out ThingConversion conversion) && Rand.Chance(conversion.chance))
                {
                    Log.Message(affecter + " has outcome for " + thing + " |: " + conversion.GetOutcome());
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
            var tibGrid = tibInfo.GetGrid();
            return tibGrid.affectedCells.ActiveCells;
        }

        public void Notfiy_TibChanged()
        {
            dirtyIterator = true;
        }

        public void SetRadiation(IntVec3 pos, double value)
        {
            hediffGrid.SetRadiation(pos, value);
        }

        public void SetInfection(IntVec3 pos, double value)
        {
            hediffGrid.SetInfection(pos, value);
        }
    }
}
