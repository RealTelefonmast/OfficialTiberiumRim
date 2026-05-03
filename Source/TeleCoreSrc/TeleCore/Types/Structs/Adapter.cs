using Verse;

namespace TeleCore.Unsorted;

public struct Adapter
{
    public ThingDef lockDef;
    public ThingDef keyDef;

    public bool Connects(Adapter other)
    {
        return lockDef == other.keyDef && keyDef == other.lockDef;
    }
}