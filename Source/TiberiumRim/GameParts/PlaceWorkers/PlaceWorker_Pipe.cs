using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_Pipe : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.GetThingList(map).Any(p => p.TryGetComp<Comp_NetworkStructure>() != null))
            {
                return false;
            }
            return true;
        }
    }
}
