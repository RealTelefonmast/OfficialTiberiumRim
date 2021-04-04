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
        public WikiEntryDef wikiEntry;

        public void Discover()
        {
            TRUtils.DiscoveryTable().Discover(this);
        }
    }

    public class DiscoveryTable : IExposable
    {
        public Dictionary<DiscoveryDef, bool> Discoveries = new Dictionary<DiscoveryDef, bool>();
        public Dictionary<TResearchDef, bool> DiscoveredResearch = new Dictionary<TResearchDef, bool>();
        public Dictionary<TRThingDef, bool> DiscoveredMenuOptions = new Dictionary<TRThingDef, bool>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Discoveries, "discoveredDict");
            Scribe_Collections.Look(ref DiscoveredResearch, "discoveredResearch");
            Scribe_Collections.Look(ref DiscoveredMenuOptions, "menuDiscovered");
        }

        //Build Menu
        public bool MenuOptionHasBeenSeen(TRThingDef def)
        {
            return DiscoveredMenuOptions.TryGetValue(def, out bool value) && value;
        }

        public void DiscoverInMenu(TRThingDef def)
        {
            if (MenuOptionHasBeenSeen(def)) return;
            DiscoveredMenuOptions.Add(def, true);
        }

        //Parent Discovery
        public bool IsDiscovered(DiscoveryDef discovery)
        {
            return Discoveries.TryGetValue(discovery, out bool value) && value;
        }

        public bool IsDiscovered(IDiscoverable discoverable)
        {
            return IsDiscovered(discoverable.DiscoveryDef);
        }

        public void Discover(DiscoveryDef discovery)
        {
            if(IsDiscovered(discovery)) return;
            Discoveries.Add(discovery, true);
            Find.LetterStack.ReceiveLetter("TR_NewDiscovery".Translate(), "TR_NewDiscoveryDesc".Translate(discovery.description), TiberiumDefOf.DiscoveryLetter);
        }

        //Research Discovery
        public bool ResearchHasBeenSeen(TResearchDef research)
        {
            return DiscoveredResearch.TryGetValue(research, out bool value) && value;
        }

        public void DiscoverResearch(TResearchDef research)
        {
            if (ResearchHasBeenSeen(research)) return;
            DiscoveredResearch.Add(research, true);
        }
    }
}
