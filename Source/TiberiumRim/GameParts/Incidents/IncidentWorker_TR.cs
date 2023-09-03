using System.Linq;
using RimWorld;
using Verse;

namespace TR
{
    //The base incidentworker for the mod, it will take care of new features such as event handling, position gathering, etc..
    public class IncidentWorker_TR : IncidentWorker
    {
        public new TiberiumIncidentDef def => (TiberiumIncidentDef) base.def;

        protected virtual LookTargets EventTargets { get; }

        public override float BaseChanceThisGame
        {
            get
            {
                return base.BaseChanceThisGame;
            }
        }

        public override bool CanFireNowSub(IncidentParms parms)
        {
            return true;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            StartEvents(parms);
            return true;
        }

        protected void StartEvents(IncidentParms parms)
        {
            if (def.eventsToTrigger == null) return;
            foreach (var eventDef in def.eventsToTrigger)
            {
                TRUtils.EventManager().StartEvent(eventDef, EventTargets);
            }
            SendEventLetter(parms, EventTargets);
        }

        public void SendEventLetter(IncidentParms parms, LookTargets lookTargets)
        {
            TaggedString letterLabel = parms.customLetterLabel ?? def.letterLabel;
            TaggedString letterText = parms.customLetterText ?? def.letterText;
            LetterDef letterDef = parms.customLetterDef ?? def.letterDef;

            if (letterLabel.NullOrEmpty() || letterText.NullOrEmpty())
                Log.Error("Sending standard incident letter with no label or text.", false);

            //NamedArgument[] letterLabelArgs = textArgs.Select(t => t.Named("LETTERLABEL")).ToArray();
            //NamedArgument[] letterTextArgs = textArgs.Select(t => t.Named("LETTERTEXT")).ToArray();
            //TaggedString letterLabelFormatted = letterLabel.Formatted(letterLabelArgs);
            //TaggedString letterTextFormatted = letterText.Formatted(letterTextArgs);
            string addendum = LetterTextAddendum();
            if (!addendum.NullOrEmpty())
                letterText += addendum;

            EventLetter letter = (EventLetter)LetterMaker.MakeLetter(letterLabel, letterText, letterDef, lookTargets, parms.faction, parms.quest, parms.letterHyperlinkThingDefs);
            letter.AddEvents(def.eventsToTrigger);
            Find.LetterStack.ReceiveLetter(letter, null);
        }

        private string LetterTextAddendum()
        {
            string researchUnlocks = "";
            foreach (var research in def.eventsToTrigger.SelectMany(e => e.unlocksResearch))
            {
                researchUnlocks += "    -" + research.LabelCap + "\n";
            }
            if (researchUnlocks.NullOrEmpty()) return null;
            return "\n\n" + "TR_EventResearchUnlocks".Translate(researchUnlocks);
        }
    }
}
