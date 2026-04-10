using System.Linq;
using RimWorld;
using TeleCore.Research.Events;
using Verse;

namespace TR;

public class IncidentWorker_CauseEvents : IncidentWorker
{
    public TiberiumIncidentDef Def => (TiberiumIncidentDef)def;

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        foreach (var eventDef in Def.eventsToTrigger) TRUtils.EventManager().StartEvent(eventDef);

        SendEventLetter(parms, null);
        //SendStandardLetter(parms, null);
        return true;
    }

    public void SendEventLetter(IncidentParms parms, LookTargets lookTargets)
    {
        TaggedString letterLabel = parms.customLetterLabel ?? Def.letterLabel;
        TaggedString letterText = parms.customLetterText ?? Def.letterText;
        var letterDef = parms.customLetterDef ?? Def.letterDef;

        if (letterLabel.NullOrEmpty() || letterText.NullOrEmpty())
            Log.Error("Sending standard incident letter with no label or text.", false);

        //NamedArgument[] letterLabelArgs = textArgs.Select(t => t.Named("LETTERLABEL")).ToArray();
        //NamedArgument[] letterTextArgs = textArgs.Select(t => t.Named("LETTERTEXT")).ToArray();
        //TaggedString letterLabelFormatted = letterLabel.Formatted(letterLabelArgs);
        //TaggedString letterTextFormatted = letterText.Formatted(letterTextArgs);
        var addendum = LetterTextAddendum();
        if (!addendum.NullOrEmpty())
            letterText += addendum;

        var letter = (EventLetter)LetterMaker.MakeLetter(letterLabel, letterText, letterDef, lookTargets, parms.faction,
            parms.quest, parms.letterHyperlinkThingDefs);
        letter.AddEvents(Def.eventsToTrigger);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private string LetterTextAddendum()
    {
        var researchUnlocks = "";
        foreach (var research in Def.eventsToTrigger.SelectMany(e => e.unlocksResearch))
            researchUnlocks += "    -" + research.LabelCap + "\n";
        return "\n\n" + "TR_EventResearchUnlocks".Translate(researchUnlocks);
    }
}