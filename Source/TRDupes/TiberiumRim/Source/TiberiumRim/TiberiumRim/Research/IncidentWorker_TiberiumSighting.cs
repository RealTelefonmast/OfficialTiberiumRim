using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace TiberiumRim
{
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
}
