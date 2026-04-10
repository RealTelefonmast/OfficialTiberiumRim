using TeleCore.Static;
using TeleCore.Wiki;
using Verse;

namespace TeleCore.GameData.Defs;

public class DiscoveryDef : Def
{
    public WikiEntryDef wikiEntry;

    public void Discover()
    {
        TFind.Discoveries.Discover(this);
    }
}