using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Defs;

public class DiscoveryDef : Def
{
    public WikiEntryDef wikiEntry;

    public void Discover()
    {
        TFind.Discoveries.Discover(this);
    }
}