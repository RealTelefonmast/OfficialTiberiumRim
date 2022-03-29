using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_Pipe : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var comp = loc.GetThingList(map).Select(t => t.TryGetComp<Comp_NetworkStructure>()).FirstOrDefault();
            if (comp is null) return true;

            var networks = ((checkingDef as ThingDef)?.comps.Find(c => c is CompProperties_NetworkStructure) as CompProperties_NetworkStructure)?.networks?.Select(n => n.networkDef).ToArray();
            if (comp.NetworkParts.Select(t => t.NetworkDef).Any(networks.Contains))
            {
                return false;
            }
            return true;
        }
    }
}
