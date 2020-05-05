using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class DiscoveryTable : IExposable
    {
        public Dictionary<string, bool> Discovered = new Dictionary<string, bool>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Discovered, "discovered");
        }

        public bool IsDiscovered(string discoverTag)
        {
            return Discovered.TryGetValue(discoverTag, out bool value) && value;
        }

        public bool IsDiscovered(IDiscoverable discoverable)
        {
            return Discovered.TryGetValue(discoverable.DiscoverTag, out bool value) && value;
        }

        public void Discover(string discoverTag)
        {
            Discovered.Add(discoverTag, true);
            //Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(TiberiumDefOf.DiscoveryLetter));
            Find.LetterStack.ReceiveLetter("TR_NewDiscovery".Translate(), "TR_NewDiscoveryDesc".Translate(), TiberiumDefOf.DiscoveryLetter);
        }
    }
}
