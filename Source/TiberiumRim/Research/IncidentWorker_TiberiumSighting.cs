using RimWorld;

namespace TR;

public class IncidentWorker_TiberiumSighting : IncidentWorker_CauseEvents
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        return base.TryExecuteWorker(parms);
    }
}