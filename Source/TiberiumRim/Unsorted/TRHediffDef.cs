using Verse;

namespace TR;

public class TRHediffDef : HediffDef
{
    [Unsaved] private TaggedString cachedUnknownLabelCap = null;

    public DiscoveryProperties discovery;

    public bool isNaturalInsertion;

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