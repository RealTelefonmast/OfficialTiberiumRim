using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IncidentWorker_ChronoAnomaly : IncidentWorker
    {
        private Map map;
        private IntVec3 spawnLoc;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (base.CanFireNowSub(parms))
            {
                return true;
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            map = (Map) parms.target;
            if (!DropCellFinder.TryFindRaidDropCenterClose(out spawnLoc, map, false, false, true, -1))
            {
                spawnLoc = DropCellFinder.FindRaidDropCenterDistant(map, false);
            }

            ActionComposition composition = new ActionComposition("VolkovArrival");
            composition.AddPart(delegate { TRUtils.CameraPanNLock().PanDirect(spawnLoc, 5); }, 0);
            composition.AddPart(delegate
            {
                ChronoVortex Vortex = (ChronoVortex) ThingMaker.MakeThing(RedAlertDefOf.ChronoVortexPortal);
                Vortex.Add(VolkovGenerator.GenerateVolkov(map));
                Vortex.PortalSetup(8f.SecondsToTicks(), 7f.SecondsToTicks());
                GenSpawn.Spawn(Vortex, spawnLoc, map);

            }, 5f);
            composition.Init();
            return true;
        }
    }
}
