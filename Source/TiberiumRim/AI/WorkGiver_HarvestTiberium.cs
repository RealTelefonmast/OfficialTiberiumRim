using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_HarvestTiberium : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override bool Prioritized => true;
        public override bool AllowUnreachable => false;


        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (pawn is Harvester harvester)
            {
                return harvester.HarvestMode == HarvestMode.Moss ? harvester.Map.Tiberium().MossAvailable : harvester.Map.Tiberium().TiberiumAvailable;
            }
            return true;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (pawn is Harvester harvester)
            {
                TRLog.Debug($"Getting work things for {harvester.NameFullColored}| Has Zone: {harvester.RefineryComp.HarvestTiberiumZone != null} | {harvester.RefineryComp.HarvestTiberiumZone?.Cells.Count}");
                if (harvester.Container.Full) return null;
                var manager = pawn.Map.GetComponent<MapComponent_Tiberium>();
                if (HarvesterUsesZone(harvester, out _))
                {
                    TRLog.Debug($"[{pawn}] Selecting crystals inside of zone");
                    return harvester.RefineryComp.HarvestTiberiumZone.AllContainedThings.Where(t => t is TiberiumCrystal);
                }
                return manager.TiberiumInfo.AllTiberiumCrystals;
            }
            return null;
        }

        private bool HarvesterUsesZone(Pawn pawn, out IEnumerable<IntVec3> cells)
        {
            cells = null;
            if (pawn is Harvester harvester)
            {
                if (harvester.RefineryComp.HarvestTiberiumZone != null)
                {
                    if (harvester.RefineryComp.HarvestTiberiumZone.cells.Count == 0)
                    {
                        Log.ErrorOnce($"Harvest zone has 0 cells: {harvester.RefineryComp.HarvestTiberiumZone}", -674964);
                    }
                    cells = harvester.RefineryComp.HarvestTiberiumZone.cells;
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            if (HarvesterUsesZone(pawn, out var cells))
            {
                TRLog.Debug($"[{pawn.NameShortColored}] Should harvest inside of zone with cells: {cells.EnumerableCount()}");
                return cells;
            }
            return null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var harvester = pawn as Harvester;
            var crystal = t as TiberiumCrystal;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return null;
            }

            if ((crystal?.CanBeHarvestedBy(harvester) ?? false) && harvester.CanReserveAndReach(crystal, PathEndMode.ClosestTouch, Danger.Deadly))
                return JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, crystal);
            return null;
        }
    }
}
