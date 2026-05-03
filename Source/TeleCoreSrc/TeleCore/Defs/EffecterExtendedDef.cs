using TeleCore.Comps;
using Verse;

namespace TeleCore.Unsorted;

public class EffecterExtendedDef : EffecterDef
{
    public Effecter_FX SpawnWithFX(CompFX fxComp)
    {
        return new Effecter_FX(fxComp, this);
    }
}