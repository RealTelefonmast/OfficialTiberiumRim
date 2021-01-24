using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
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
