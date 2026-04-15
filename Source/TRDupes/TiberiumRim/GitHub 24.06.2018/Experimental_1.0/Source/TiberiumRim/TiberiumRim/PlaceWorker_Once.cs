using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_Once : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            string defName = checkingDef.defName;
            Thing thing = map.listerThings.AllThings.Find((Thing x) => x.def.defName.Contains(defName));
            if (thing != null)
            {
                return "ThingAlreadyExists".Translate(new object[] {
                    checkingDef.LabelCap
                });
            }
            return true;
        }
    }
}
