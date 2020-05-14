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
        public Dictionary<TRThingDef, bool> TRMenuDiscovered = new Dictionary<TRThingDef, bool>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Discovered, "discovered");
            Scribe_Collections.Look(ref TRMenuDiscovered, "menuDiscovered");
        }

        public bool IsMenuDiscovered(TRThingDef def)
        {
            return TRMenuDiscovered.TryGetValue(def, out bool value) && value;
        }

        public void DiscoverMenu(TRThingDef def)
        {
            TRMenuDiscovered.Add(def, true);
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
