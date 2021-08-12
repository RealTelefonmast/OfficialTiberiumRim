using RimWorld;

namespace TiberiumRim
{
    public class IncidentWorker_TiberiumSighting : IncidentWorker_TR
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!TRUtils.Tiberium().AllowTRInit) return false;
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return base.TryExecuteWorker(parms);
        }
    }
}
