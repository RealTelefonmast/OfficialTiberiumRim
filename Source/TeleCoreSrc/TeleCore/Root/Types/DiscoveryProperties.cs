using TeleCore.GameData.Defs;
using Verse;

namespace TeleCore.Types;

public class DiscoveryProperties : Editable
{
    [Unsaved()]
    private TaggedString cachedUnknownLabelCap = null;

    public DiscoveryDef discoveryDef;
    public string extraDescription;
    public string unknownDescription;
    public string unknownLabel;

    public string UnknownLabelCap
    {
        get
        {
            if (cachedUnknownLabelCap.NullOrEmpty())
                cachedUnknownLabelCap = unknownLabel.CapitalizeFirst();
            return cachedUnknownLabelCap;
        }
    }
}