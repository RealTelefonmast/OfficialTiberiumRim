using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IncidentWorker_CauseEvents : IncidentWorker
    {
        public TiberiumIncidentDef IncidentDef => (TiberiumIncidentDef) base.def;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            foreach (var eventDef in IncidentDef.eventsToTrigger)
            {
                TRUtils.EventManager().TriggerEvent(eventDef);
            }
            SendStandardLetter(IncidentDef.letterLabel, LetterTextAddendum(), IncidentDef.letterDef, parms, null);
            //SendStandardLetter(parms, null);
            return true;
        }

        private string LetterTextAddendum()
        {
            string text = IncidentDef.letterText + "\n";
            text += "TR_IncidentUnlocks".Translate() + "\n";
            foreach (var research in IncidentDef.eventsToTrigger.SelectMany(e => e.unlocksResearch))
            {
                text += "    -" + research.LabelCap + "\n";
            }
            return text;
        }
    }
}
