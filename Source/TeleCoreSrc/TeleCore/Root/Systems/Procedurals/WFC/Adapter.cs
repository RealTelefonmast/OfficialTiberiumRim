using Verse;

namespace TeleCore.Systems.Procedurals.WFC;

public struct Adapter
{
    public ThingDef lockDef;
    public ThingDef keyDef;

    public bool Connects(Adapter other)
    {
        return lockDef == other.keyDef && keyDef == other.lockDef;
    }
}