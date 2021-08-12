using RimWorld;
using Verse;

namespace TiberiumRim
{
    //This will spawn additional tiberium meteorites
    public class IncidentWorker_TiberiumArrival : IncidentWorker_TR
    {
        private ThingDef innerThing;
        private Thing skyfallerThing;

        protected override LookTargets EventTargets => skyfallerThing;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Check if needed research is done
            if (!TRUtils.Tiberium().AllowNewMeteorites) return false;
            //Check if map is oversaturated with ProducerContainers

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!CanFireNowSub(parms)) return false;
            Map map = (Map) parms.target;

            var pair = def.skyfallers.RandomElementByWeight(s => s.chance);
            innerThing = pair.innerThing;

            if (!TryFindCell(out IntVec3 cell, map)) return false;
            var faller = SkyfallerMaker.MakeSkyfaller(pair.skyfallerDef, pair.innerThing);
            skyfallerThing = faller.innerContainer[0];

            GenSpawn.Spawn(faller, cell, map);
            SendStandardLetter(parms, skyfallerThing);
            return true;
        }

        protected bool TryFindCell(out IntVec3 cell, Map map)
        {
            return CellFinderLoose.TryFindSkyfallerCell(innerThing, map, out cell, 20, default(IntVec3), -1, true, true,
                false, false, false, false, x => CellUtils.AllowTiberiumMeteorite(x, map));
        }
    }
}
