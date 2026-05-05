using RimWorld;
using Verse;

namespace TeleCore.Research;

public class LetterProperties
{
    public LetterDef letterDef;
    public string letterLabel;
    public string letterText;

    public void SendLetter(IncidentParms parms = null, LookTargets targets = null)
    {
        var letter = LetterMaker.MakeLetter(letterLabel, letterText, letterDef, targets, parms?.faction, parms?.quest,
            parms?.letterHyperlinkThingDefs);
        Find.LetterStack.ReceiveLetter(letter);
    }
}