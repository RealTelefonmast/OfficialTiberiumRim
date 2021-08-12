using Verse;

namespace TiberiumRim
{
    public class TRHediffDef : HediffDef
    {
        [Unsaved(false)]
        private TaggedString cachedUnknownLabelCap = null;

        public bool isNaturalInsertion;
        public DiscoveryProperties discovery;

        public string UnknownLabelCap
        {
            get
            {
                if (cachedUnknownLabelCap.NullOrEmpty())
                    cachedUnknownLabelCap = discovery.unknownLabel.CapitalizeFirst();
                return cachedUnknownLabelCap;
            }
        }
    }
}
