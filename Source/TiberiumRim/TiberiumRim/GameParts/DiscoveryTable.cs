using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class DiscoveryDef : Def
    {
    }

    public class DiscoveryTable : IExposable
    {
        public Dictionary<DiscoveryDef, bool> Discovered = new Dictionary<DiscoveryDef, bool>();
        public Dictionary<TRThingDef, bool> TRMenuDiscovered = new Dictionary<TRThingDef, bool>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Discovered, "discoveredDict");
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

        public bool IsDiscovered(DiscoveryDef discovery)
        {
            return Discovered.TryGetValue(discovery, out bool value) && value;
        }

        public bool IsDiscovered(IDiscoverable discoverable)
        {
            return IsDiscovered(discoverable.DiscoveryDef);
        }

        public void Discover(DiscoveryDef discovery)
        {
            Discovered.Add(discovery, true);
            Find.LetterStack.ReceiveLetter("TR_NewDiscovery".Translate(), "TR_NewDiscoveryDesc".Translate(discovery.description), TiberiumDefOf.DiscoveryLetter);
        }
    }
}
