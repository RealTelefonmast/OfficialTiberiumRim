using TeleCore.ThingComps;
using Verse;

namespace TeleCore.Visual.VFX.Effecters;

public class EffecterExtendedDef : EffecterDef
{
    public Effecter_FX SpawnWithFX(CompFX fxComp)
    {
        return new Effecter_FX(fxComp, this);
    }
}