using RimWorld;
using Verse;

namespace TR
{
    public class LetterProperties
    {
        public string letterLabel;
        public string letterText;
        public LetterDef letterDef;

        public void SendLetter(IncidentParms parms = null, LookTargets targets = null)
        {
            var letter = LetterMaker.MakeLetter(letterLabel, letterText, letterDef, targets, parms?.faction, parms?.quest, parms?.letterHyperlinkThingDefs);
            Find.LetterStack.ReceiveLetter(letter);
        }
    }
}
